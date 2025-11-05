using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace discussionspot9.Services
{
    public interface IStoryGenerationService
    {
        Task QueueStoryGenerationAsync(int postId, StoryGenerationOptions options);
        Task ProcessStoryGenerationAsync(int postId, StoryGenerationOptions options);
    }

    public class StoryGenerationService : IStoryGenerationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<StoryGenerationService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly PythonStoryEnhancerService _pythonEnhancer;

        public StoryGenerationService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<StoryGenerationService> logger,
            IBackgroundTaskQueue taskQueue,
            PythonStoryEnhancerService pythonEnhancer)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _taskQueue = taskQueue;
            _pythonEnhancer = pythonEnhancer;
        }

        public async Task QueueStoryGenerationAsync(int postId, StoryGenerationOptions options)
        {
            try
            {
                // Add to background queue
                _taskQueue.QueueBackgroundWorkItemAsync(async token =>
                {
                    await ProcessStoryGenerationAsync(postId, options);
                });

                _logger.LogInformation($"Story generation queued for post {postId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error queuing story generation for post {postId}");
            }
        }

        public async Task ProcessStoryGenerationAsync(int postId, StoryGenerationOptions options)
        {
            try
            {
                _logger.LogInformation($"Starting story generation for post {postId}");

                // Get the post
                using var context = _contextFactory.CreateDbContext();
                var post = await context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.UserProfile)
                    .Include(p => p.Media)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    _logger.LogWarning($"Post {postId} not found for story generation");
                    return;
                }

                // Create story from post
                var story = await CreateStoryFromPostAsync(post, options, context);
                
                if (story != null)
                {
                    _logger.LogInformation($"Story created successfully for post {postId} with ID {story.StoryId}");
                    
                    // Try to enhance with Python AI if enabled
                    if (options.UseAI)
                    {
                        await EnhanceStoryWithPythonAIAsync(story, post, options, context);
                    }
                    
                    // PHASE 2: Auto-generate alt text for images
                    await GenerateMissingAltTextForStoryAsync(story, post, context);
                    
                    // PHASE 3: Ensure proper thumbnails/images in story
                    await EnsureProperThumbnailsAndImagesAsync(story, post, context);
                    
                    // PHASE 4: Validate story quality before publishing
                    var validationResult = await ValidateStoryQualityAsync(story, context);
                    
                    // PHASE 1: Auto-publish story immediately (after all enhancements and validation)
                    if (validationResult.IsValid)
                    {
                        await AutoPublishStoryAsync(story, context);
                    }
                    else
                    {
                        _logger.LogWarning($"Story {story.StoryId} failed quality validation and will remain as draft. " +
                                          $"Issues: {string.Join(", ", validationResult.Issues)}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing story generation for post {postId}");
            }
        }

        private async Task<Story?> CreateStoryFromPostAsync(Post post, StoryGenerationOptions options, ApplicationDbContext context)
        {
            try
            {
                // Analyze post performance for optimization
                var performanceData = await AnalyzePostPerformanceAsync(post.PostId, context);
                
                // Generate story title with SEO optimization
                var storyTitle = GenerateStoryTitle(post.Title, options.Style);
                
                // Enhanced content extraction
                var extractedContent = ExtractContentFromPost(post, options);
                
                // Generate SEO-optimized description
                var metaDescription = GenerateSEOOptimizedDescription(extractedContent, post, options);
                var keywords = GenerateSEOKeywords(post, options, context);
                
                // PHASE 3: Determine poster image URL from post media
                var posterImageUrl = DeterminePosterImageUrl(post, extractedContent);
                
                // Create story entity with enhanced SEO
                var story = new Story
                {
                    Title = storyTitle,
                    Slug = GenerateStorySlug(storyTitle),
                    Description = GenerateStoryDescription(extractedContent.MainContent, options),
                    PostId = post.PostId, // CRITICAL: Link back to original post
                    UserId = post.UserId,
                    CommunityId = post.CommunityId,
                    Status = "draft", // Will be auto-published after validation (Phase 1)
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsAmpEnabled = true,
                    PublisherName = post.Community?.Title ?? "DiscussionSpot",
                    PublisherLogo = "/Assets/Logo_Auth.png",
                    PosterImageUrl = posterImageUrl, // PHASE 3: Set poster image for thumbnail
                    MetaDescription = metaDescription,
                    MetaKeywords = keywords,
                    CanonicalUrl = $"/stories/amp/{GenerateStorySlug(storyTitle)}"
                };

                context.Stories.Add(story);
                await context.SaveChangesAsync();

                // Generate slides with hybrid approach
                await GenerateStorySlidesAsync(story, post, options, context, performanceData);

                return story;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating story from post {post.PostId}");
                return null;
            }
        }

        private async Task<PostPerformanceData> AnalyzePostPerformanceAsync(int postId, ApplicationDbContext context)
        {
            try
            {
                var post = await context.Posts.FindAsync(postId);
                if (post == null) return new PostPerformanceData();
                
                // Analyze engagement patterns
                var viewCount = post.ViewCount;
                var commentCount = post.CommentCount;
                var upvoteCount = post.UpvoteCount;
                var score = post.Score;
                
                // Calculate engagement rate
                var engagementRate = viewCount > 0 
                    ? (double)(commentCount + upvoteCount) / viewCount 
                    : 0;
                
                // Get similar successful stories
                var successfulStories = await context.Stories
                    .Where(s => s.Status == "published" && s.ViewCount > 100)
                    .OrderByDescending(s => s.ViewCount)
                    .Take(10)
                    .Select(s => new { s.SlideCount, s.TotalDuration })
                    .ToListAsync();
                
                var avgSlideCount = successfulStories.Any() 
                    ? (int)successfulStories.Average(s => s.SlideCount)
                    : 8;
                
                var avgDuration = successfulStories.Any()
                    ? (int)successfulStories.Average(s => s.TotalDuration)
                    : 35000;
                
                return new PostPerformanceData
                {
                    EngagementRate = engagementRate,
                    ViewCount = viewCount,
                    OptimalSlideCount = avgSlideCount,
                    OptimalDuration = avgDuration,
                    IsHighEngagement = engagementRate > 0.1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing post performance for post {postId}");
                return new PostPerformanceData();
            }
        }
        
        private ContentExtractionResult ExtractContentFromPost(Post post, StoryGenerationOptions options)
        {
            var result = new ContentExtractionResult();
            
            // Extract main content
            result.MainContent = post.Content ?? "";
            
            // Extract key points (rule-based)
            result.KeyPoints = ExtractKeyPoints(result.MainContent);
            
            // Extract quotes
            result.Quotes = ExtractQuotes(result.MainContent);
            
            // Extract questions
            result.Questions = ExtractQuestions(result.MainContent);
            
            // Extract media URLs
            result.MediaUrls = post.Media?.Select(m => m.Url ?? "").Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>();
            
            // Extract tags
            result.Tags = post.PostTags?.Select(pt => pt.Tag?.Name ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>();
            
            // Extract links
            result.Links = ExtractLinks(result.MainContent);
            
            return result;
        }
        
        private List<string> ExtractKeyPoints(string content)
        {
            if (string.IsNullOrEmpty(content)) return new List<string>();
            
            var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 20 && s.Length < 200)
                .Take(10)
                .ToList();
            
            return sentences;
        }
        
        private List<string> ExtractQuotes(string content)
        {
            if (string.IsNullOrEmpty(content)) return new List<string>();
            
            var quotes = new List<string>();
            var quoteStart = content.IndexOf('"');
            
            while (quoteStart >= 0)
            {
                var quoteEnd = content.IndexOf('"', quoteStart + 1);
                if (quoteEnd > quoteStart)
                {
                    var quote = content.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                    if (quote.Length > 10 && quote.Length < 200)
                    {
                        quotes.Add(quote);
                    }
                    quoteStart = content.IndexOf('"', quoteEnd + 1);
                }
                else
                {
                    break;
                }
            }
            
            return quotes;
        }
        
        private List<string> ExtractQuestions(string content)
        {
            if (string.IsNullOrEmpty(content)) return new List<string>();
            
            return content.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(q => q.Trim() + "?")
                .Where(q => q.Length > 10 && q.Length < 200)
                .Take(5)
                .ToList();
        }
        
        private List<string> ExtractLinks(string content)
        {
            if (string.IsNullOrEmpty(content)) return new List<string>();
            
            var links = new List<string>();
            var urlPattern = new System.Text.RegularExpressions.Regex(@"https?://[^\s]+");
            var matches = urlPattern.Matches(content);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                links.Add(match.Value);
            }
            
            return links;
        }
        
        private string GenerateSEOOptimizedDescription(ContentExtractionResult content, Post post, StoryGenerationOptions options)
        {
            // Use meta description if available
            if (!string.IsNullOrEmpty(options.Keywords))
            {
                var keywordPhrase = options.Keywords.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(keywordPhrase))
                {
                    var mainContent = content.MainContent ?? "";
                    var contentLength = Math.Min(120, mainContent.Length);
                    var snippet = mainContent.Length > 120 ? mainContent.Substring(0, contentLength) : mainContent;
                    return $"{post.Title} - {keywordPhrase}. {snippet}...";
                }
            }
            
            // Generate from content
            var description = content.MainContent?.Length > 160 
                ? content.MainContent.Substring(0, 160) + "..."
                : content.MainContent ?? $"Discover {post.Title} on DiscussionSpot";
            
            // Add community context if available
            if (post.Community != null)
            {
                description = $"{description} - Join the discussion in {post.Community.Title}";
            }
            
            return description.Length > 160 ? description.Substring(0, 160) + "..." : description;
        }
        
        private string GenerateSEOKeywords(Post post, StoryGenerationOptions options, ApplicationDbContext context)
        {
            var keywords = new List<string>();
            
            // Use provided keywords
            if (!string.IsNullOrEmpty(options.Keywords))
            {
                keywords.AddRange(options.Keywords.Split(',').Select(k => k.Trim()));
            }
            
            // Extract from title
            var titleKeywords = ExtractKeywords(post.Title);
            keywords.AddRange(titleKeywords.Split(',').Select(k => k.Trim()));
            
            // Add tags
            if (post.PostTags != null)
            {
                keywords.AddRange(post.PostTags.Select(pt => pt.Tag?.Name ?? "").Where(t => !string.IsNullOrEmpty(t)));
            }
            
            // Add community name
            if (post.Community != null)
            {
                keywords.Add(post.Community.Title);
            }
            
            // Add common story keywords
            keywords.Add("web story");
            keywords.Add("amp story");
            keywords.Add("visual story");
            
            // Remove duplicates and take top 10
            return string.Join(", ", keywords.Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
        }

        private async Task GenerateStorySlidesAsync(Story story, Post post, StoryGenerationOptions options, ApplicationDbContext context, PostPerformanceData? performanceData = null)
        {
            try
            {
                var slides = new List<StorySlide>();
                
                // Use optimal slide count from performance data if available
                var slideCount = performanceData?.OptimalSlideCount ?? GetSlideCount(options.Length);

                // Title slide with enhanced SEO
                slides.Add(new StorySlide
                {
                    StoryId = story.StoryId,
                    Caption = story.Title,
                    Headline = story.Title,
                    Text = story.Description,
                    Duration = 3000,
                    OrderIndex = 0,
                    SlideType = "title",
                    BackgroundColor = "#667eea",
                    TextColor = "#ffffff",
                    FontSize = "24",
                    Alignment = "center",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                // Content slides based on post type with hybrid approach
                var extractedContent = ExtractContentFromPost(post, options);
                
                switch (post.PostType)
                {
                    case "text":
                        await GenerateEnhancedTextSlidesAsync(slides, post, slideCount, extractedContent, options);
                        break;
                    case "link":
                        await GenerateLinkSlidesAsync(slides, post, slideCount);
                        break;
                    case "image":
                        await GenerateEnhancedImageSlidesAsync(slides, post, slideCount, extractedContent);
                        break;
                    case "poll":
                        await GeneratePollSlidesAsync(slides, post, slideCount);
                        break;
                    default:
                        await GenerateEnhancedTextSlidesAsync(slides, post, slideCount, extractedContent, options);
                        break;
                }

                // Add final CTA slide with link back to post
                var postUrl = $"/r/{post.Community?.Slug}/posts/{post.Slug}";
                slides.Add(new StorySlide
                {
                    StoryId = story.StoryId,
                    Caption = "Read more on DiscussionSpot",
                    Headline = "💬 Join the Discussion",
                    Text = $"View full post and comments",
                    Duration = 4000,
                    OrderIndex = slides.Count,
                    SlideType = "cta",
                    BackgroundColor = "#007bff",
                    TextColor = "#ffffff",
                    FontSize = "18",
                    Alignment = "center",
                    MediaUrl = postUrl,
                    MediaType = "internal_link",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                // Add slides to database
                context.StorySlides.AddRange(slides);
                await context.SaveChangesAsync();

                // Update story with slide count and duration
                story.SlideCount = slides.Count;
                story.TotalDuration = slides.Sum(s => s.Duration);
                story.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating slides for story {story.StoryId}");
            }
        }

        private async Task GenerateTextSlidesAsync(List<StorySlide> slides, Post post, int slideCount)
        {
            if (string.IsNullOrEmpty(post.Content)) return;

            var content = post.Content;
            var sentences = content.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var slidesToCreate = Math.Min(slideCount - 1, sentences.Length);

            for (int i = 0; i < slidesToCreate; i++)
            {
                if (i < sentences.Length)
                {
                    slides.Add(new StorySlide
                    {
                        StoryId = slides[0].StoryId,
                        Caption = sentences[i].Trim(),
                        Headline = $"Key Point {i + 1}",
                        Text = sentences[i].Trim(),
                        Duration = 4,
                        OrderIndex = i + 1,
                        SlideType = "text",
                        BackgroundColor = "#f8f9fa",
                        TextColor = "#333333",
                        FontSize = "18",
                        Alignment = "left"
                    });
                }
            }
        }

        private async Task GenerateLinkSlidesAsync(List<StorySlide> slides, Post post, int slideCount)
        {
            if (string.IsNullOrEmpty(post.Url)) return;

            // Add URL slide with clickable link
            slides.Add(new StorySlide
            {
                StoryId = slides[0].StoryId,
                Caption = "Check out this link",
                Headline = "📎 External Link",
                Text = $"Visit: {post.Url}",
                Duration = 5,
                OrderIndex = 1,
                SlideType = "link",
                BackgroundColor = "#28a745",
                TextColor = "#ffffff",
                FontSize = "20",
                Alignment = "center",
                MediaUrl = post.Url,
                MediaType = "external_link"
            });

            // Add CTA slide to visit the link
            slides.Add(new StorySlide
            {
                StoryId = slides[0].StoryId,
                Caption = "Click to explore",
                Headline = "🔗 Tap to Visit",
                Text = "Swipe up to open the link in browser",
                Duration = 3,
                OrderIndex = 2,
                SlideType = "cta",
                BackgroundColor = "#007bff",
                TextColor = "#ffffff",
                FontSize = "18",
                Alignment = "center",
                MediaUrl = post.Url,
                MediaType = "external_link",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        private async Task GenerateEnhancedTextSlidesAsync(List<StorySlide> slides, Post post, int slideCount, ContentExtractionResult extractedContent, StoryGenerationOptions options)
        {
            if (string.IsNullOrEmpty(post.Content) && !extractedContent.KeyPoints.Any()) return;

            var slideIndex = 1;
            
            // Add key points as slides
            foreach (var keyPoint in extractedContent.KeyPoints.Take(slideCount - 1))
            {
                slides.Add(new StorySlide
                {
                    StoryId = slides[0].StoryId,
                    Caption = keyPoint,
                    Headline = $"Key Point {slideIndex}",
                    Text = keyPoint,
                    Duration = CalculateOptimalDuration(keyPoint, 4000),
                    OrderIndex = slideIndex,
                    SlideType = "text",
                    BackgroundColor = GetBackgroundColorForStyle(options.Style),
                    TextColor = "#333333",
                    FontSize = "18",
                    Alignment = "left",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                slideIndex++;
            }
            
            // Add quotes if available
            if (slideIndex < slideCount && extractedContent.Quotes.Any())
            {
                foreach (var quote in extractedContent.Quotes.Take(slideCount - slideIndex))
                {
                    slides.Add(new StorySlide
                    {
                        StoryId = slides[0].StoryId,
                        Caption = quote,
                        Headline = "💬 Quote",
                        Text = $"\"{quote}\"",
                        Duration = CalculateOptimalDuration(quote, 5000),
                        OrderIndex = slideIndex,
                        SlideType = "quote",
                        BackgroundColor = "#f8f9fa",
                        TextColor = "#333333",
                        FontSize = "20",
                        Alignment = "center",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    slideIndex++;
                }
            }
        }
        
        private async Task GenerateEnhancedImageSlidesAsync(List<StorySlide> slides, Post post, int slideCount, ContentExtractionResult extractedContent)
        {
            var mediaItems = post.Media?.ToList() ?? new List<Media>();
            var slideIndex = 1;
            
            foreach (var media in mediaItems.Take(slideCount - 1))
            {
                // PHASE 3: Ensure proper MediaUrl and use thumbnail if available
                var mediaUrl = media.Url;
                if (string.IsNullOrWhiteSpace(mediaUrl) && !string.IsNullOrWhiteSpace(media.ThumbnailUrl))
                {
                    mediaUrl = media.ThumbnailUrl;
                }
                
                // Ensure we have a valid media URL
                if (string.IsNullOrWhiteSpace(mediaUrl))
                {
                    _logger.LogWarning($"Media {media.MediaId} has no URL or thumbnail, skipping slide creation");
                    continue;
                }
                
                // PHASE 2: Use alt text if available, otherwise generate placeholder
                // Note: Alt text will be generated later in Phase 2, but we set a placeholder here
                var altText = media.AltText;
                if (string.IsNullOrWhiteSpace(altText))
                {
                    // Generate a simple placeholder - Phase 2 will replace with AI-generated alt text
                    altText = media.Caption ?? $"Image {slideIndex} from {post.Title}";
                }
                
                slides.Add(new StorySlide
                {
                    StoryId = slides[0].StoryId,
                    MediaId = media.MediaId,
                    Caption = media.Caption ?? extractedContent.KeyPoints.FirstOrDefault() ?? "Visual Content",
                    Headline = slideIndex == 1 ? "Visual Content" : $"Image {slideIndex}",
                    Text = altText ?? (!string.IsNullOrEmpty(extractedContent.MainContent) ? extractedContent.MainContent.Substring(0, Math.Min(100, extractedContent.MainContent.Length)) : "Check out this image"),
                    Duration = 5000,
                    OrderIndex = slideIndex,
                    SlideType = "image",
                    BackgroundColor = "#6c757d",
                    TextColor = "#ffffff",
                    FontSize = "18",
                    Alignment = "center",
                    MediaUrl = mediaUrl, // PHASE 3: Use proper URL
                    MediaType = media.MediaType ?? "image",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                slideIndex++;
            }
        }
        
        private string GetBackgroundColorForStyle(string style)
        {
            return style switch
            {
                "engaging" => "#ff6b6b",
                "educational" => "#4ecdc4",
                "entertaining" => "#ff9a9e",
                "informative" => "#667eea",
                _ => "#f8f9fa"
            };
        }

        private async Task GeneratePollSlidesAsync(List<StorySlide> slides, Post post, int slideCount)
        {
            slides.Add(new StorySlide
            {
                StoryId = slides[0].StoryId,
                Caption = "What do you think?",
                Headline = "Poll Question",
                Text = "Vote in the poll below!",
                Duration = 4,
                OrderIndex = 1,
                SlideType = "poll",
                BackgroundColor = "#ffc107",
                TextColor = "#000000",
                FontSize = "20",
                Alignment = "center"
            });
        }

        private string GenerateStoryTitle(string postTitle, string style)
        {
            var prefix = style switch
            {
                "informative" => "📚 ",
                "engaging" => "🎯 ",
                "tutorial" => "🔧 ",
                "news" => "📰 ",
                "entertainment" => "🎭 ",
                _ => "📖 "
            };

            return $"{prefix}{postTitle}";
        }

        private string GenerateStorySlug(string title)
        {
            return title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("📚", "")
                .Replace("🎯", "")
                .Replace("🔧", "")
                .Replace("📰", "")
                .Replace("🎭", "")
                .Replace("📖", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("\"", "")
                .Replace("'", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("|", "-")
                .Replace("+", "-")
                .Replace("=", "-")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("&", "")
                .Replace("*", "")
                .Replace("~", "")
                .Replace("`", "")
                .Trim('-');
        }

        private string GenerateStoryDescription(string? content, StoryGenerationOptions options)
        {
            if (string.IsNullOrEmpty(content))
                return "A Web Story created from a DiscussionSpot post";

            var description = content.Length > 200 
                ? content.Substring(0, 200) + "..." 
                : content;

            return $"{description} - Generated with {options.Style} style";
        }

        private string GenerateMetaDescription(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return "Discover this engaging Web Story on DiscussionSpot";

            return content.Length > 160 
                ? content.Substring(0, 160) + "..." 
                : content;
        }

        private string ExtractKeywords(string title)
        {
            return title.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 3)
                .Take(5)
                .Aggregate((a, b) => $"{a}, {b}");
        }

        private int GetSlideCount(string length)
        {
            return length switch
            {
                "short" => 5,
                "long" => 15,
                _ => 10 // medium
            };
        }

        private async Task EnhanceStoryWithPythonAIAsync(Story story, Post post, StoryGenerationOptions options, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Enhancing story {story.StoryId} with Python AI");
                
                // Check if Python is available
                if (!await _pythonEnhancer.IsPythonAvailableAsync())
                {
                    _logger.LogWarning("Python is not available, falling back to basic enhancement");
                    await EnhanceStoryWithBasicAIAsync(story, post, options, context);
                    return;
                }

                if (!await _pythonEnhancer.IsScriptAvailableAsync())
                {
                    _logger.LogWarning("Python script is not available, falling back to basic enhancement");
                    await EnhanceStoryWithBasicAIAsync(story, post, options, context);
                    return;
                }

                // Prepare content for Python enhancement
                var contentInput = new StoryContentInput
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    PostType = post.PostType,
                    Tags = post.PostTags?.Select(pt => pt.Tag?.Name).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>(),
                    MediaUrls = post.Media?.Select(m => m.Url).ToList() ?? new List<string>(),
                    CommunityName = post.Community?.Title ?? "",
                    AuthorName = post.UserProfile?.DisplayName ?? post.User?.UserName ?? "Unknown"
                };

                var optionsInput = new StoryOptionsInput
                {
                    Style = options.Style,
                    Length = options.Length,
                    UseAI = options.UseAI,
                    Keywords = options.Keywords,
                    AutoGenerate = options.AutoGenerate
                };

                // Get enhanced slides from Python
                var enhancedSlides = await _pythonEnhancer.EnhanceStoryAsync(contentInput, optionsInput);

                // Clear existing slides
                var existingSlides = await context.StorySlides
                    .Where(s => s.StoryId == story.StoryId)
                    .ToListAsync();
                context.StorySlides.RemoveRange(existingSlides);

                // Add enhanced slides
                var maxOrderIndex = enhancedSlides.Any() ? enhancedSlides.Max(s => s.OrderIndex) : -1;
                foreach (var slideData in enhancedSlides)
                {
                    var slide = new StorySlide
                    {
                        StoryId = story.StoryId,
                        OrderIndex = slideData.OrderIndex,
                        SlideType = slideData.SlideType,
                        Headline = slideData.Headline,
                        Text = slideData.Text,
                        Caption = slideData.Caption,
                        BackgroundColor = slideData.BackgroundColor,
                        TextColor = slideData.TextColor,
                        Alignment = slideData.Alignment,
                        FontSize = slideData.FontSize,
                        Duration = slideData.Duration,
                        MediaUrl = slideData.MediaUrl,
                        MediaType = slideData.MediaType
                    };
                    context.StorySlides.Add(slide);
                }

                // CRITICAL: Always add final CTA slide with link back to post (even after Python enhancement)
                var postUrl = $"/r/{post.Community?.Slug}/posts/{post.Slug}";
                var finalCtaSlide = new StorySlide
                {
                    StoryId = story.StoryId,
                    Caption = "Read more on DiscussionSpot",
                    Headline = "💬 Join the Discussion",
                    Text = "View full post and comments",
                    Duration = 4000,
                    OrderIndex = maxOrderIndex + 1, // Add after all Python-generated slides
                    SlideType = "cta",
                    BackgroundColor = "#007bff",
                    TextColor = "#ffffff",
                    FontSize = "18",
                    Alignment = "center",
                    MediaUrl = postUrl, // CRITICAL: Link to original post
                    MediaType = "internal_link", // CRITICAL: Mark as internal link
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.StorySlides.Add(finalCtaSlide);

                await context.SaveChangesAsync();
                _logger.LogInformation($"Python AI enhancement completed for story {story.StoryId} with {enhancedSlides.Count} slides + final CTA slide");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enhancing story {story.StoryId} with Python AI, falling back to basic enhancement");
                await EnhanceStoryWithBasicAIAsync(story, post, options, context);
            }
        }

        private async Task EnhanceStoryWithBasicAIAsync(Story story, Post post, StoryGenerationOptions options, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Enhancing story {story.StoryId} with basic AI");
                
                // Get existing slides
                var existingSlides = await context.StorySlides
                    .Where(s => s.StoryId == story.StoryId)
                    .OrderBy(s => s.OrderIndex)
                    .ToListAsync();

                if (!existingSlides.Any())
                {
                    _logger.LogWarning($"No slides found for story {story.StoryId}");
                    return;
                }

                // Basic AI enhancement logic
                foreach (var slide in existingSlides)
                {
                    // Enhance headline
                    slide.Headline = EnhanceHeadline(slide.Headline, options.Style);
                    
                    // Enhance text
                    slide.Text = EnhanceText(slide.Text, options.Style);
                    
                    // Optimize colors
                    OptimizeColors(slide, options.Style);
                    
                    // Adjust duration based on content
                    slide.Duration = CalculateOptimalDuration(slide.Text, slide.Duration);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Basic AI enhancement completed for story {story.StoryId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enhancing story {story.StoryId} with basic AI");
            }
        }

        private string EnhanceHeadline(string headline, string style)
        {
            if (string.IsNullOrEmpty(headline)) return headline;
            
            return style switch
            {
                "engaging" => headline.StartsWith("Did you know") ? headline : $"Did you know: {headline}",
                "educational" => headline.StartsWith("Learn") ? headline : $"Learn: {headline}",
                "entertaining" => headline.Contains("🎉") ? headline : $"🎉 {headline}",
                _ => headline
            };
        }

        private string EnhanceText(string text, string style)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            // Simple text enhancement based on style
            return style switch
            {
                "engaging" => text.EndsWith("?") ? text : $"{text}?",
                "educational" => text.StartsWith("Step") ? text : $"Step: {text}",
                _ => text
            };
        }

        private void OptimizeColors(StorySlide slide, string style)
        {
            // Basic color optimization
            if (string.IsNullOrEmpty(slide.BackgroundColor))
            {
                slide.BackgroundColor = style switch
                {
                    "engaging" => "#ff6b6b",
                    "educational" => "#4ecdc4",
                    "entertaining" => "#ff9a9e",
                    _ => "#667eea"
                };
            }
            
            if (string.IsNullOrEmpty(slide.TextColor))
            {
                slide.TextColor = "#ffffff";
            }
        }

        private int CalculateOptimalDuration(string text, int currentDuration)
        {
            if (string.IsNullOrEmpty(text)) return currentDuration;
            
            var wordCount = text.Split(' ').Length;
            // Base duration: 3 seconds + 0.1 seconds per word, max 8 seconds
            var optimalDuration = Math.Max(3000, Math.Min(8000, 3000 + wordCount * 100));
            return optimalDuration;
        }

        private async Task UpdateStoryWithAIContentAsync(Story story, List<AISlide> aiSlides, ApplicationDbContext context)
        {
            try
            {
                // Remove existing slides
                var existingSlides = await context.StorySlides
                    .Where(s => s.StoryId == story.StoryId)
                    .ToListAsync();
                
                context.StorySlides.RemoveRange(existingSlides);

                // Add AI-generated slides
                var newSlides = aiSlides.Select(aiSlide => new StorySlide
                {
                    StoryId = story.StoryId,
                    Caption = aiSlide.Caption,
                    Headline = aiSlide.Headline,
                    Text = aiSlide.Text,
                    Duration = aiSlide.Duration,
                    OrderIndex = aiSlide.OrderIndex,
                    SlideType = aiSlide.SlideType,
                    BackgroundColor = aiSlide.BackgroundColor,
                    TextColor = aiSlide.TextColor,
                    FontSize = aiSlide.FontSize.ToString(),
                    Alignment = aiSlide.Alignment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                context.StorySlides.AddRange(newSlides);

                // Update story metadata
                story.SlideCount = newSlides.Count;
                story.TotalDuration = newSlides.Sum(s => s.Duration);
                story.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                _logger.LogInformation($"Updated story {story.StoryId} with {newSlides.Count} AI-generated slides");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating story {story.StoryId} with AI content");
            }
        }

        /// <summary>
        /// PHASE 3: Ensure proper thumbnails/images in story slides and poster image
        /// </summary>
        private async Task EnsureProperThumbnailsAndImagesAsync(Story story, Post post, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Ensuring proper thumbnails and images for story {story.StoryId}");
                
                // Reload story with slides and post with media
                story = await context.Stories
                    .Include(s => s.Slides)
                    .FirstOrDefaultAsync(s => s.StoryId == story.StoryId);
                
                if (story == null)
                {
                    _logger.LogWarning($"Story {story.StoryId} not found for thumbnail enhancement");
                    return;
                }
                
                var updatedCount = 0;
                
                // 1. Ensure all slides have proper MediaUrl
                if (story.Slides != null && story.Slides.Any())
                {
                    foreach (var slide in story.Slides)
                    {
                        // If slide has MediaId but no MediaUrl, fetch it
                        if (slide.MediaId.HasValue && string.IsNullOrWhiteSpace(slide.MediaUrl))
                        {
                            var media = await context.Media.FindAsync(slide.MediaId.Value);
                            if (media != null)
                            {
                                // Prefer thumbnail URL if available, otherwise use full URL
                                slide.MediaUrl = !string.IsNullOrWhiteSpace(media.ThumbnailUrl) 
                                    ? media.ThumbnailUrl 
                                    : media.Url;
                                
                                if (string.IsNullOrWhiteSpace(slide.MediaType))
                                {
                                    slide.MediaType = media.MediaType ?? "image";
                                }
                                
                                updatedCount++;
                                _logger.LogDebug($"Updated slide {slide.StorySlideId} with MediaUrl: {slide.MediaUrl}");
                            }
                        }
                        // If slide has MediaUrl but no MediaType, infer from URL
                        else if (!string.IsNullOrWhiteSpace(slide.MediaUrl) && string.IsNullOrWhiteSpace(slide.MediaType))
                        {
                            var url = slide.MediaUrl.ToLowerInvariant();
                            if (url.Contains(".jpg") || url.Contains(".jpeg") || url.Contains(".png") || 
                                url.Contains(".gif") || url.Contains(".webp") || url.Contains(".svg"))
                            {
                                slide.MediaType = "image";
                            }
                            else if (url.Contains(".mp4") || url.Contains(".webm") || url.Contains(".mov"))
                            {
                                slide.MediaType = "video";
                            }
                            else
                            {
                                slide.MediaType = "image"; // Default to image
                            }
                            updatedCount++;
                        }
                    }
                }
                
                // 2. Ensure story has PosterImageUrl (thumbnail)
                if (string.IsNullOrWhiteSpace(story.PosterImageUrl))
                {
                    // Priority 1: First image from post media (prefer thumbnail)
                    var firstPostImage = post.Media?
                        .Where(m => m.MediaType == "image" && (!string.IsNullOrWhiteSpace(m.Url) || !string.IsNullOrWhiteSpace(m.ThumbnailUrl)))
                        .FirstOrDefault();
                    
                    if (firstPostImage != null)
                    {
                        story.PosterImageUrl = !string.IsNullOrWhiteSpace(firstPostImage.ThumbnailUrl) 
                            ? firstPostImage.ThumbnailUrl 
                            : firstPostImage.Url;
                        
                        updatedCount++;
                        _logger.LogDebug($"Set story {story.StoryId} PosterImageUrl from post media: {story.PosterImageUrl}");
                    }
                    // Priority 2: First slide with image
                    else if (story.Slides != null && story.Slides.Any())
                    {
                        var firstImageSlide = story.Slides
                            .Where(s => !string.IsNullOrWhiteSpace(s.MediaUrl) && 
                                       (s.MediaType == "image" || s.SlideType == "image"))
                            .OrderBy(s => s.OrderIndex)
                            .FirstOrDefault();
                        
                        if (firstImageSlide != null)
                        {
                            story.PosterImageUrl = firstImageSlide.MediaUrl;
                            updatedCount++;
                            _logger.LogDebug($"Set story {story.StoryId} PosterImageUrl from slide: {story.PosterImageUrl}");
                        }
                    }
                }
                
                // 3. Validate and fix any slides with missing MediaUrl for image slides
                if (story.Slides != null && story.Slides.Any())
                {
                    var imageSlidesWithoutUrl = story.Slides
                        .Where(s => (s.SlideType == "image" || s.MediaType == "image") && 
                                   string.IsNullOrWhiteSpace(s.MediaUrl))
                        .ToList();
                    
                    foreach (var slide in imageSlidesWithoutUrl)
                    {
                        if (slide.MediaId.HasValue)
                        {
                            var media = await context.Media.FindAsync(slide.MediaId.Value);
                            if (media != null && !string.IsNullOrWhiteSpace(media.Url))
                            {
                                slide.MediaUrl = !string.IsNullOrWhiteSpace(media.ThumbnailUrl) 
                                    ? media.ThumbnailUrl 
                                    : media.Url;
                                slide.MediaType = media.MediaType ?? "image";
                                updatedCount++;
                                _logger.LogDebug($"Fixed missing MediaUrl for slide {slide.StorySlideId}");
                            }
                        }
                    }
                }
                
                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Phase 3: Updated {updatedCount} items for story {story.StoryId} (thumbnails/images)");
                }
                else
                {
                    _logger.LogInformation($"Phase 3: Story {story.StoryId} already has proper thumbnails/images");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ensuring thumbnails/images for story {story.StoryId}");
                // Don't fail story creation if thumbnail enhancement fails
            }
        }
        
        /// <summary>
        /// PHASE 1: Auto-publish story immediately after creation
        /// </summary>
        private async Task AutoPublishStoryAsync(Story story, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Auto-publishing story {story.StoryId}");
                
                // Reload story with slides to ensure we have latest data
                story = await context.Stories
                    .Include(s => s.Slides)
                    .FirstOrDefaultAsync(s => s.StoryId == story.StoryId);
                
                if (story == null)
                {
                    _logger.LogWarning($"Story {story.StoryId} not found for auto-publishing");
                    return;
                }
                
                // Basic checks before publishing
                var hasSlides = story.Slides?.Any() == true;
                var hasTitle = !string.IsNullOrWhiteSpace(story.Title);
                var hasSlug = !string.IsNullOrWhiteSpace(story.Slug) && story.Slug.Length > 3;
                var isLinkedToPost = story.PostId.HasValue;
                
                // Only publish if basic requirements are met
                if (hasSlides && hasTitle && hasSlug && isLinkedToPost)
                {
                    story.Status = "published";
                    story.PublishedAt = DateTime.UtcNow;
                    story.UpdatedAt = DateTime.UtcNow;
                    
                    // PHASE 3: Final check - ensure poster image is set (fallback)
                    if (string.IsNullOrWhiteSpace(story.PosterImageUrl) && story.Slides != null)
                    {
                        var firstImageSlide = story.Slides
                            .Where(s => !string.IsNullOrWhiteSpace(s.MediaUrl) && 
                                       (s.MediaType == "image" || s.SlideType == "image"))
                            .OrderBy(s => s.OrderIndex)
                            .FirstOrDefault();
                        
                        if (firstImageSlide != null)
                        {
                            story.PosterImageUrl = firstImageSlide.MediaUrl;
                            _logger.LogDebug($"Set poster image during publish for story {story.StoryId}: {story.PosterImageUrl}");
                        }
                    }
                    
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation($"✅ Story {story.StoryId} auto-published successfully. Title: {story.Title}, Slides: {story.Slides?.Count ?? 0}, PosterImage: {!string.IsNullOrWhiteSpace(story.PosterImageUrl)}");
                }
                else
                {
                    _logger.LogWarning($"Story {story.StoryId} not auto-published - missing requirements. " +
                                     $"HasSlides: {hasSlides}, HasTitle: {hasTitle}, HasSlug: {hasSlug}, LinkedToPost: {isLinkedToPost}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error auto-publishing story {story.StoryId}");
                // Don't fail story creation if auto-publish fails - keep as draft
            }
        }
        
        /// <summary>
        /// PHASE 2: Auto-generate alt text for images in story and post
        /// </summary>
        private async Task GenerateMissingAltTextForStoryAsync(Story story, Post post, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Generating missing alt text for story {story.StoryId}");
                
                // Get all media associated with the post
                var postMedia = await context.Media
                    .Where(m => m.PostId == post.PostId && m.MediaType == "image")
                    .ToListAsync();
                
                // Get all media referenced in story slides
                var storySlides = await context.StorySlides
                    .Where(s => s.StoryId == story.StoryId && s.MediaId != null)
                    .ToListAsync();
                
                var mediaIds = storySlides.Select(s => s.MediaId.Value).Distinct().ToList();
                var storyMedia = await context.Media
                    .Where(m => mediaIds.Contains(m.MediaId) && m.MediaType == "image")
                    .ToListAsync();
                
                // Combine all media
                var allMedia = postMedia.Union(storyMedia).DistinctBy(m => m.MediaId).ToList();
                
                int optimizedCount = 0;
                
                // Generate alt text for each media without alt text
                foreach (var media in allMedia)
                {
                    if (string.IsNullOrWhiteSpace(media.AltText) || media.AltText.Length < 10)
                    {
                        // Generate alt text using AI
                        var altText = await GenerateAIImageAltTextAsync(media, post, story);
                        
                        if (!string.IsNullOrWhiteSpace(altText))
                        {
                            media.AltText = altText;
                            optimizedCount++;
                            _logger.LogDebug($"Generated alt text for media {media.MediaId}: {altText}");
                        }
                    }
                }
                
                if (optimizedCount > 0)
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Generated alt text for {optimizedCount} images in story {story.StoryId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating alt text for story {story.StoryId}");
                // Don't fail story creation if alt text generation fails
            }
        }
        
        /// <summary>
        /// Generate AI-powered alt text for an image
        /// </summary>
        private async Task<string> GenerateAIImageAltTextAsync(Media media, Post post, Story story)
        {
            try
            {
                // Extract keywords from post title and content
                var keywords = ExtractKeywords(post.Title);
                var titleWords = post.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3)
                    .Take(3)
                    .ToList();
                
                // Build descriptive alt text
                var altTextParts = new List<string>();
                
                // Add primary keyword or first significant word from title
                if (titleWords.Any())
                {
                    altTextParts.Add(titleWords.First().ToLowerInvariant());
                }
                else
                {
                    altTextParts.Add("image");
                }
                
                // Add context from post title (truncated)
                if (!string.IsNullOrEmpty(post.Title))
                {
                    var titleSnippet = post.Title.Length > 50 
                        ? post.Title.Substring(0, 50).Trim() 
                        : post.Title;
                    altTextParts.Add($"related to {titleSnippet.ToLowerInvariant()}");
                }
                
                // Add media type context if available
                if (!string.IsNullOrEmpty(media.Caption))
                {
                    var captionSnippet = media.Caption.Length > 30 
                        ? media.Caption.Substring(0, 30).Trim() 
                        : media.Caption;
                    altTextParts.Add($"showing {captionSnippet.ToLowerInvariant()}");
                }
                
                // Combine parts
                var altText = string.Join(" ", altTextParts);
                
                // Ensure it's descriptive and not too short
                if (altText.Length < 10)
                {
                    altText = $"Image showing content from {post.Title}";
                }
                
                // Limit to 125 characters (SEO best practice)
                if (altText.Length > 125)
                {
                    altText = altText.Substring(0, 122) + "...";
                }
                
                // Clean up: remove multiple spaces, special characters that might break HTML
                altText = System.Text.RegularExpressions.Regex.Replace(altText, @"\s+", " ");
                altText = altText.Replace("\"", "'").Trim();
                
                return altText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating AI alt text for media {media.MediaId}");
                // Fallback to simple alt text
                return $"Image from {post.Title}";
            }
        }
        
        /// <summary>
        /// PHASE 3: Determine poster image URL for story thumbnail
        /// </summary>
        private string? DeterminePosterImageUrl(Post post, ContentExtractionResult extractedContent)
        {
            try
            {
                // Priority 1: First image from post media (prefer thumbnail if available)
                var firstImageMedia = post.Media?
                    .Where(m => m.MediaType == "image" && (!string.IsNullOrWhiteSpace(m.Url) || !string.IsNullOrWhiteSpace(m.ThumbnailUrl)))
                    .FirstOrDefault();
                
                if (firstImageMedia != null)
                {
                    // Prefer thumbnail URL if available, otherwise use full URL
                    return !string.IsNullOrWhiteSpace(firstImageMedia.ThumbnailUrl) 
                        ? firstImageMedia.ThumbnailUrl 
                        : firstImageMedia.Url;
                }
                
                // Priority 2: First media URL from extracted content
                var firstMediaUrl = extractedContent.MediaUrls?
                    .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url) && 
                                           (url.Contains(".jpg") || url.Contains(".jpeg") || 
                                            url.Contains(".png") || url.Contains(".gif") || 
                                            url.Contains(".webp")));
                
                if (!string.IsNullOrWhiteSpace(firstMediaUrl))
                {
                    return firstMediaUrl;
                }
                
                // Priority 3: Any media URL from extracted content
                if (extractedContent.MediaUrls?.Any() == true)
                {
                    return extractedContent.MediaUrls.First();
                }
                
                // No poster image found - return null (will use default in UI)
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining poster image URL for post {PostId}", post.PostId);
                return null;
            }
        }
        
        /// <summary>
        /// PHASE 4: Validate story quality before publishing
        /// </summary>
        private async Task<StoryQualityValidationResult> ValidateStoryQualityAsync(Story story, ApplicationDbContext context)
        {
            var result = new StoryQualityValidationResult();
            
            try
            {
                _logger.LogInformation($"Validating story quality for story {story.StoryId}");
                
                // Reload story with slides to get latest data
                story = await context.Stories
                    .Include(s => s.Slides)
                    .FirstOrDefaultAsync(s => s.StoryId == story.StoryId);
                
                if (story == null)
                {
                    result.Issues.Add("Story not found in database");
                    return result;
                }
                
                // 1. Check minimum slide count (at least 3 slides for quality story)
                var slideCount = story.Slides?.Count ?? 0;
                if (slideCount < 3)
                {
                    result.Issues.Add($"Insufficient slides: {slideCount} (minimum 3 required)");
                }
                else
                {
                    result.SlideCount = slideCount;
                }
                
                // 2. Check if story has title
                if (string.IsNullOrWhiteSpace(story.Title))
                {
                    result.Issues.Add("Story title is missing");
                }
                else if (story.Title.Length < 10)
                {
                    result.Issues.Add($"Story title is too short: {story.Title.Length} characters (minimum 10 recommended)");
                }
                else
                {
                    result.HasTitle = true;
                }
                
                // 3. Check if story has description
                if (string.IsNullOrWhiteSpace(story.Description))
                {
                    result.Issues.Add("Story description is missing");
                }
                else if (story.Description.Length < 20)
                {
                    result.Issues.Add($"Story description is too short: {story.Description.Length} characters (minimum 20 recommended)");
                }
                else
                {
                    result.HasDescription = true;
                }
                
                // 4. Check if story has valid slug
                if (string.IsNullOrWhiteSpace(story.Slug))
                {
                    result.Issues.Add("Story slug is missing");
                }
                else if (story.Slug.Length < 3)
                {
                    result.Issues.Add($"Story slug is too short: {story.Slug.Length} characters (minimum 3 required)");
                }
                else
                {
                    result.HasValidSlug = true;
                }
                
                // 5. Check if story has at least one slide with content
                var hasContentSlides = story.Slides?.Any(s => 
                    (!string.IsNullOrWhiteSpace(s.Headline) || 
                     !string.IsNullOrWhiteSpace(s.Text) || 
                     !string.IsNullOrWhiteSpace(s.MediaUrl))) ?? false;
                
                if (!hasContentSlides)
                {
                    result.Issues.Add("No slides with content (headline, text, or media) found");
                }
                else
                {
                    result.HasContentSlides = true;
                }
                
                // 6. Check if story is linked to a post (required for auto-publishing)
                if (!story.PostId.HasValue)
                {
                    result.Issues.Add("Story is not linked to a post (PostId is missing)");
                }
                else
                {
                    result.IsLinkedToPost = true;
                }
                
                // 7. Check if story has poster image (thumbnail) - warning, not critical
                if (string.IsNullOrWhiteSpace(story.PosterImageUrl))
                {
                    result.Warnings.Add("Story poster image (thumbnail) is missing - will use default in listings");
                }
                else
                {
                    result.HasPosterImage = true;
                }
                
                // 8. Check slide quality - ensure slides have meaningful content
                if (story.Slides != null && story.Slides.Any())
                {
                    var emptySlides = story.Slides.Count(s => 
                        string.IsNullOrWhiteSpace(s.Headline) && 
                        string.IsNullOrWhiteSpace(s.Text) && 
                        string.IsNullOrWhiteSpace(s.MediaUrl));
                    
                    if (emptySlides > 0)
                    {
                        result.Warnings.Add($"{emptySlides} slide(s) have no content (headline, text, or media)");
                    }
                    
                    // Check for slides with very short content
                    var shortContentSlides = story.Slides.Count(s => 
                        (!string.IsNullOrWhiteSpace(s.Text) && s.Text.Length < 10) ||
                        (!string.IsNullOrWhiteSpace(s.Headline) && s.Headline.Length < 5));
                    
                    if (shortContentSlides > 0)
                    {
                        result.Warnings.Add($"{shortContentSlides} slide(s) have very short content");
                    }
                }
                
                // 9. Check total duration - ensure story has reasonable duration
                if (story.TotalDuration < 5000)
                {
                    result.Warnings.Add($"Story total duration is very short: {story.TotalDuration}ms (minimum 5 seconds recommended)");
                }
                else
                {
                    result.TotalDuration = story.TotalDuration;
                }
                
                // Determine if story is valid for publishing
                // Critical issues must be resolved, warnings are acceptable
                result.IsValid = result.Issues.Count == 0;
                
                if (result.IsValid)
                {
                    _logger.LogInformation($"✅ Story {story.StoryId} passed quality validation. " +
                                         $"Slides: {result.SlideCount}, " +
                                         $"Duration: {result.TotalDuration}ms, " +
                                         $"PosterImage: {result.HasPosterImage}, " +
                                         $"Warnings: {result.Warnings.Count}");
                }
                else
                {
                    _logger.LogWarning($"Story {story.StoryId} failed quality validation. " +
                                     $"Issues: {result.Issues.Count}, " +
                                     $"Warnings: {result.Warnings.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating story quality for story {story.StoryId}");
                result.Issues.Add($"Validation error: {ex.Message}");
                result.IsValid = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// PHASE 4 (Legacy): Validate story quality and auto-publish if criteria are met
        /// </summary>
        private async Task ValidateAndPublishStoryAsync(Story story, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Validating story {story.StoryId} for auto-publishing");
                
                // Reload story with slides to get latest data
                story = await context.Stories
                    .Include(s => s.Slides)
                    .FirstOrDefaultAsync(s => s.StoryId == story.StoryId);
                
                if (story == null)
                {
                    _logger.LogWarning($"Story {story.StoryId} not found for validation");
                    return;
                }
                
                // Validation criteria
                var validationResult = new StoryValidationResult();
                
                // 1. Check minimum slide count (at least 3 slides)
                var slideCount = story.Slides?.Count ?? 0;
                validationResult.HasMinimumSlides = slideCount >= 3;
                
                if (!validationResult.HasMinimumSlides)
                {
                    _logger.LogWarning($"Story {story.StoryId} has only {slideCount} slides, minimum required is 3");
                }
                
                // 2. Check if story has title
                validationResult.HasTitle = !string.IsNullOrWhiteSpace(story.Title);
                
                // 3. Check if story has description
                validationResult.HasDescription = !string.IsNullOrWhiteSpace(story.Description);
                
                // 4. Check if story has at least one slide with content
                var hasContentSlides = story.Slides?.Any(s => 
                    (!string.IsNullOrWhiteSpace(s.Headline) || 
                     !string.IsNullOrWhiteSpace(s.Text) || 
                     !string.IsNullOrWhiteSpace(s.MediaUrl))) ?? false;
                validationResult.HasContentSlides = hasContentSlides;
                
                // 5. Check if story has proper slug
                validationResult.HasValidSlug = !string.IsNullOrWhiteSpace(story.Slug) && story.Slug.Length > 3;
                
                // 6. Check if story is linked to a post (required for auto-publishing)
                validationResult.IsLinkedToPost = story.PostId.HasValue;
                
                // Determine if story should be auto-published
                var shouldPublish = validationResult.HasMinimumSlides &&
                                   validationResult.HasTitle &&
                                   validationResult.HasDescription &&
                                   validationResult.HasContentSlides &&
                                   validationResult.HasValidSlug &&
                                   validationResult.IsLinkedToPost;
                
                if (shouldPublish)
                {
                    // PHASE 1: Auto-publish the story
                    story.Status = "published";
                    story.PublishedAt = DateTime.UtcNow; // Set published timestamp
                    story.UpdatedAt = DateTime.UtcNow;
                    
                    // PHASE 3: Ensure poster image is set if not already set
                    if (string.IsNullOrWhiteSpace(story.PosterImageUrl))
                    {
                        // Try to get first slide with media
                        var firstSlideWithMedia = story.Slides?
                            .Where(s => !string.IsNullOrWhiteSpace(s.MediaUrl) && 
                                       (s.MediaType == "image" || s.SlideType == "image"))
                            .OrderBy(s => s.OrderIndex)
                            .FirstOrDefault();
                        
                        if (firstSlideWithMedia != null)
                        {
                            story.PosterImageUrl = firstSlideWithMedia.MediaUrl;
                            _logger.LogDebug($"Set poster image for story {story.StoryId} from slide: {story.PosterImageUrl}");
                        }
                    }
                    
                    await context.SaveChangesAsync();
                    
                    _logger.LogInformation($"✅ Story {story.StoryId} auto-published successfully. Slides: {slideCount}, Title: {story.Title}, PosterImage: {!string.IsNullOrWhiteSpace(story.PosterImageUrl)}");
                }
                else
                {
                    _logger.LogWarning($"Story {story.StoryId} did not meet auto-publish criteria. " +
                                     $"MinSlides: {validationResult.HasMinimumSlides}, " +
                                     $"HasTitle: {validationResult.HasTitle}, " +
                                     $"HasDesc: {validationResult.HasDescription}, " +
                                     $"HasContent: {validationResult.HasContentSlides}, " +
                                     $"HasSlug: {validationResult.HasValidSlug}, " +
                                     $"LinkedToPost: {validationResult.IsLinkedToPost}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating story {story.StoryId} for auto-publishing");
                // Don't fail story creation if validation fails - keep as draft
            }
        }

    public class PostPerformanceData
    {
        public double EngagementRate { get; set; }
        public int ViewCount { get; set; }
        public int OptimalSlideCount { get; set; } = 8;
        public int OptimalDuration { get; set; } = 35000;
        public bool IsHighEngagement { get; set; }
    }
    
    public class ContentExtractionResult
    {
        public string MainContent { get; set; } = "";
        public List<string> KeyPoints { get; set; } = new();
        public List<string> Quotes { get; set; } = new();
        public List<string> Questions { get; set; } = new();
        public List<string> MediaUrls { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public List<string> Links { get; set; } = new();
    }

    public class AIStoryResponse
    {
        public bool Success { get; set; }
        public List<AISlide>? Slides { get; set; }
        public string? Error { get; set; }
    }

    public class AISlide
    {
        public string Caption { get; set; } = "";
        public string Headline { get; set; } = "";
        public string Text { get; set; } = "";
        public int Duration { get; set; }
        public string SlideType { get; set; } = "";
        public string BackgroundColor { get; set; } = "";
        public string TextColor { get; set; } = "";
        public int FontSize { get; set; }
        public string Alignment { get; set; } = "";
        public int OrderIndex { get; set; }
    }

    public class StoryGenerationOptions
    {
        public bool AutoGenerate { get; set; } = true;
        public bool UseAI { get; set; } = true;
        public string Style { get; set; } = "informative";
        public string Length { get; set; } = "medium";
        public string? Keywords { get; set; }
    }

    public class StoryValidationResult
    {
        public bool HasMinimumSlides { get; set; }
        public bool HasTitle { get; set; }
        public bool HasDescription { get; set; }
        public bool HasContentSlides { get; set; }
        public bool HasValidSlug { get; set; }
        public bool IsLinkedToPost { get; set; }
    }
    
    public class StoryQualityValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        
        // Detailed validation results
        public int SlideCount { get; set; }
        public bool HasTitle { get; set; }
        public bool HasDescription { get; set; }
        public bool HasValidSlug { get; set; }
        public bool HasContentSlides { get; set; }
        public bool IsLinkedToPost { get; set; }
        public bool HasPosterImage { get; set; }
        public int TotalDuration { get; set; }
    }
}
