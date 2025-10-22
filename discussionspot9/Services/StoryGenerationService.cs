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

        public StoryGenerationService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<StoryGenerationService> logger,
            IBackgroundTaskQueue taskQueue)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _taskQueue = taskQueue;
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
                    
                    // Try to enhance with AI if enabled
                    if (options.UseAI)
                    {
                        await EnhanceStoryWithAIAsync(story, post, options, context);
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
                // Generate story title
                var storyTitle = GenerateStoryTitle(post.Title, options.Style);
                
                // Create story entity
                var story = new Story
                {
                    Title = storyTitle,
                    Slug = GenerateStorySlug(storyTitle),
                    Description = GenerateStoryDescription(post.Content, options),
                    UserId = post.UserId,
                    CommunityId = post.CommunityId,
                    Status = "draft",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsAmpEnabled = true,
                    PublisherName = "DiscussionSpot",
                    PublisherLogo = "/Assets/Logo_Auth.png",
                    MetaDescription = GenerateMetaDescription(post.Content),
                    MetaKeywords = options.Keywords ?? ExtractKeywords(post.Title)
                };

                context.Stories.Add(story);
                await context.SaveChangesAsync();

                // Generate slides
                await GenerateStorySlidesAsync(story, post, options, context);

                return story;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating story from post {post.PostId}");
                return null;
            }
        }

        private async Task GenerateStorySlidesAsync(Story story, Post post, StoryGenerationOptions options, ApplicationDbContext context)
        {
            try
            {
                var slides = new List<StorySlide>();
                var slideCount = GetSlideCount(options.Length);

                // Title slide
                slides.Add(new StorySlide
                {
                    StoryId = story.StoryId,
                    Caption = story.Title,
                    Headline = story.Title,
                    Text = story.Description,
                    Duration = 3,
                    OrderIndex = 0,
                    SlideType = "title",
                    BackgroundColor = "#007bff",
                    TextColor = "#ffffff",
                    FontSize = "24",
                    Alignment = "center"
                });

                // Content slides based on post type
                switch (post.PostType)
                {
                    case "text":
                        await GenerateTextSlidesAsync(slides, post, slideCount);
                        break;
                    case "link":
                        await GenerateLinkSlidesAsync(slides, post, slideCount);
                        break;
                    case "image":
                        await GenerateImageSlidesAsync(slides, post, slideCount);
                        break;
                    case "poll":
                        await GeneratePollSlidesAsync(slides, post, slideCount);
                        break;
                }

                // Add slides to database
                context.StorySlides.AddRange(slides);
                await context.SaveChangesAsync();

                // Update story with slide count
                story.SlideCount = slides.Count;
                story.TotalDuration = slides.Sum(s => s.Duration);
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

            slides.Add(new StorySlide
            {
                StoryId = slides[0].StoryId,
                Caption = "Check out this link",
                Headline = "External Link",
                Text = post.Url,
                Duration = 3,
                OrderIndex = 1,
                SlideType = "link",
                BackgroundColor = "#28a745",
                TextColor = "#ffffff",
                FontSize = "20",
                Alignment = "center"
            });
        }

        private async Task GenerateImageSlidesAsync(List<StorySlide> slides, Post post, int slideCount)
        {
            var media = post.Media?.FirstOrDefault();
            if (media == null) return;

            slides.Add(new StorySlide
            {
                StoryId = slides[0].StoryId,
                MediaId = media.MediaId,
                Caption = media.Caption ?? "Image from post",
                Headline = "Visual Content",
                Text = media.AltText ?? "Check out this image",
                Duration = 5,
                OrderIndex = 1,
                SlideType = "image",
                BackgroundColor = "#6c757d",
                TextColor = "#ffffff",
                FontSize = "18",
                Alignment = "center"
            });
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

        private async Task EnhanceStoryWithAIAsync(Story story, Post post, StoryGenerationOptions options, ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation($"Enhancing story {story.StoryId} with AI");

                // Prepare request data for Python script
                var requestData = new
                {
                    post_id = post.PostId,
                    title = post.Title,
                    content = post.Content ?? "",
                    post_type = post.PostType,
                    style = options.Style,
                    length = options.Length,
                    keywords = options.Keywords,
                    media_urls = post.Media?.Select(m => m.Url).ToList() ?? new List<string>()
                };

                // Call Python script
                var pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "story_generator.py");
                var requestJson = System.Text.Json.JsonSerializer.Serialize(requestData);
                
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScript}\" \"{requestJson}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        var response = System.Text.Json.JsonSerializer.Deserialize<AIStoryResponse>(output);
                        if (response?.Success == true && response.Slides?.Any() == true)
                        {
                            await UpdateStoryWithAIContentAsync(story, response.Slides, context);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Python script failed: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enhancing story {story.StoryId} with AI");
            }
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
