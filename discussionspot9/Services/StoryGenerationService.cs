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
                
                // Create story entity with enhanced SEO
                var story = new Story
                {
                    Title = storyTitle,
                    Slug = GenerateStorySlug(storyTitle),
                    Description = GenerateStoryDescription(extractedContent.MainContent, options),
                    PostId = post.PostId, // CRITICAL: Link back to original post
                    UserId = post.UserId,
                    CommunityId = post.CommunityId,
                    Status = "draft",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsAmpEnabled = true,
                    PublisherName = post.Community?.Title ?? "DiscussionSpot",
                    PublisherLogo = "/Assets/Logo_Auth.png",
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
            slides.Add(new StorySlide
            {
                StoryId = slides[0].StoryId,
                MediaId = media.MediaId,
                    Caption = media.Caption ?? extractedContent.KeyPoints.FirstOrDefault() ?? "Visual Content",
                    Headline = slideIndex == 1 ? "Visual Content" : $"Image {slideIndex}",
                    Text = media.AltText ?? (!string.IsNullOrEmpty(extractedContent.MainContent) ? extractedContent.MainContent.Substring(0, Math.Min(100, extractedContent.MainContent.Length)) : "Check out this image"),
                    Duration = 5000,
                    OrderIndex = slideIndex,
                SlideType = "image",
                BackgroundColor = "#6c757d",
                TextColor = "#ffffff",
                FontSize = "18",
                    Alignment = "center",
                    MediaUrl = media.Url,
                    MediaType = media.MediaType,
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
}
