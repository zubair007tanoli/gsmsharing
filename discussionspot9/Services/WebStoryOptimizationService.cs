using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using discussionspot9.Services;
using discussionspot9.Services.MCP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace discussionspot9.Services
{
    public interface IWebStoryOptimizationService
    {
        Task<WebStoryOptimizationResult> OptimizeStoryAsync(int storyId);
        Task<WebStoryValidationResult> ValidateStoryAsync(int storyId);
        Task<string> GenerateSeoOptimizedTitleAsync(string originalTitle, string? content = null);
        Task<string> GenerateSeoOptimizedDescriptionAsync(string title, string? content = null);
    }

    public class WebStoryOptimizationService : IWebStoryOptimizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WebStoryOptimizationService> _logger;
        private readonly ILocalAIService? _localAI;
        private readonly IMcpClientService? _mcpClient;

        public WebStoryOptimizationService(
            ApplicationDbContext context,
            ILogger<WebStoryOptimizationService> logger,
            ILocalAIService? localAI = null,
            IMcpClientService? mcpClient = null)
        {
            _context = context;
            _logger = logger;
            _localAI = localAI;
            _mcpClient = mcpClient;
        }

        public async Task<WebStoryOptimizationResult> OptimizeStoryAsync(int storyId)
        {
            var story = await _context.Stories
                .Include(s => s.Slides.OrderBy(sl => sl.OrderIndex))
                .Include(s => s.Post)
                .FirstOrDefaultAsync(s => s.StoryId == storyId);

            if (story == null)
            {
                return new WebStoryOptimizationResult
                {
                    Success = false,
                    ErrorMessage = "Story not found"
                };
            }

            var improvements = new List<string>();
            var warnings = new List<string>();

            try
            {
                // 1. Optimize title with AI
                if (!string.IsNullOrEmpty(story.Title))
                {
                    var optimizedTitle = await GenerateSeoOptimizedTitleAsync(story.Title, story.Description);
                    if (!string.IsNullOrEmpty(optimizedTitle) && optimizedTitle != story.Title)
                    {
                        story.Title = optimizedTitle;
                        improvements.Add("Title optimized for SEO");
                    }
                }

                // 2. Optimize meta description
                if (string.IsNullOrEmpty(story.MetaDescription))
                {
                    var metaDesc = await GenerateSeoOptimizedDescriptionAsync(story.Title ?? "", story.Description);
                    if (!string.IsNullOrEmpty(metaDesc))
                    {
                        story.MetaDescription = metaDesc;
                        improvements.Add("Meta description generated");
                    }
                }

                // 3. Validate slides
                var slideIssues = ValidateSlides(story.Slides);
                warnings.AddRange(slideIssues);

                // 4. Ensure poster images
                if (string.IsNullOrEmpty(story.PosterImageUrl))
                {
                    var firstImage = story.Slides
                        .OrderBy(s => s.OrderIndex)
                        .FirstOrDefault(s => !string.IsNullOrEmpty(s.MediaUrl) && IsImageFile(s.MediaUrl));
                    
                    if (firstImage != null)
                    {
                        story.PosterImageUrl = firstImage.MediaUrl;
                        improvements.Add("Poster image set from first slide");
                    }
                    else
                    {
                        warnings.Add("No poster image found - required for Google Web Stories");
                    }
                }

                // 5. Ensure publisher logo
                if (string.IsNullOrEmpty(story.PublisherLogo))
                {
                    story.PublisherLogo = "/Assets/Logo_Auth.png";
                    improvements.Add("Publisher logo set to default");
                }

                // 6. Validate minimum slides (at least 4 for Google)
                if (story.Slides.Count < 4)
                {
                    warnings.Add($"Story has only {story.Slides.Count} slides. Google recommends at least 4 slides.");
                }

                // 7. Check slide durations
                var shortSlides = story.Slides.Where(s => s.Duration < 3000).ToList();
                if (shortSlides.Any())
                {
                    warnings.Add($"{shortSlides.Count} slides have duration less than 3 seconds. Recommended: 3-5 seconds per slide.");
                }

                story.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new WebStoryOptimizationResult
                {
                    Success = true,
                    Improvements = improvements,
                    Warnings = warnings,
                    OptimizedTitle = story.Title,
                    OptimizedMetaDescription = story.MetaDescription
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing story {StoryId}", storyId);
                return new WebStoryOptimizationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<WebStoryValidationResult> ValidateStoryAsync(int storyId)
        {
            var story = await _context.Stories
                .Include(s => s.Slides.OrderBy(sl => sl.OrderIndex))
                .FirstOrDefaultAsync(s => s.StoryId == storyId);

            if (story == null)
            {
                return new WebStoryValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "Story not found" }
                };
            }

            var errors = new List<string>();
            var warnings = new List<string>();

            // 1. Check required fields
            if (string.IsNullOrEmpty(story.Title))
                errors.Add("Title is required");
            
            if (string.IsNullOrEmpty(story.PublisherName))
                warnings.Add("Publisher name is recommended");
            
            if (string.IsNullOrEmpty(story.PublisherLogo))
                errors.Add("Publisher logo is required");
            
            if (string.IsNullOrEmpty(story.PosterImageUrl))
                errors.Add("Poster image is required");

            // 2. Validate slides
            if (!story.Slides.Any())
            {
                errors.Add("Story must have at least one slide");
            }
            else
            {
                if (story.Slides.Count < 4)
                    warnings.Add($"Story has {story.Slides.Count} slides. Google recommends at least 4 slides.");

                var slideIssues = ValidateSlides(story.Slides);
                warnings.AddRange(slideIssues);
            }

            // 3. Check for required AMP attributes
            if (!story.IsAmpEnabled)
                warnings.Add("AMP is disabled - required for Google Web Stories");

            return new WebStoryValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }

        public async Task<string> GenerateSeoOptimizedTitleAsync(string originalTitle, string? content = null)
        {
            try
            {
                // Try MCP SEO server first
                if (_mcpClient != null)
                {
                    try
                    {
                        var result = await _mcpClient.CallSeoServerAsync<dynamic>("optimize_title", new
                        {
                            title = originalTitle,
                            context = content ?? ""
                        });
                        
                        if (result != null)
                        {
                            var optimizedTitleProp = result.GetType().GetProperty("optimized_title");
                            if (optimizedTitleProp != null)
                            {
                                var optimizedValue = optimizedTitleProp.GetValue(result);
                                if (optimizedValue is string optimized && !string.IsNullOrEmpty(optimized))
                                {
                                    return optimized;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "MCP SEO server failed, falling back to local AI");
                    }
                }

                // Fallback to local AI
                if (_localAI != null && await _localAI.IsAvailableAsync())
                {
                    var optimized = await _localAI.OptimizeTitleAsync(originalTitle, content);
                    if (!string.IsNullOrEmpty(optimized))
                    {
                        return optimized;
                    }
                }

                // Fallback: Simple optimization
                return OptimizeTitleSimple(originalTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SEO optimized title");
                return originalTitle;
            }
        }

        public async Task<string> GenerateSeoOptimizedDescriptionAsync(string title, string? content = null)
        {
            try
            {
                // Try MCP SEO server first
                if (_mcpClient != null)
                {
                    try
                    {
                        var result = await _mcpClient.CallSeoServerAsync<dynamic>("generate_meta_description", new
                        {
                            title = title,
                            content = content ?? ""
                        });
                        
                        if (result != null)
                        {
                            var metaDescProp = result.GetType().GetProperty("meta_description");
                            if (metaDescProp != null)
                            {
                                var metaDescValue = metaDescProp.GetValue(result);
                                if (metaDescValue is string metaDesc && !string.IsNullOrEmpty(metaDesc))
                                {
                                    return metaDesc;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "MCP SEO server failed, falling back to local AI");
                    }
                }

                // Fallback to local AI
                if (_localAI != null && await _localAI.IsAvailableAsync())
                {
                    var metaDesc = await _localAI.GenerateMetaDescriptionAsync(content ?? "", title);
                    if (!string.IsNullOrEmpty(metaDesc))
                    {
                        return metaDesc;
                    }
                }

                // Fallback: Generate from content
                return GenerateDescriptionSimple(title, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SEO optimized description");
                return GenerateDescriptionSimple(title, content);
            }
        }

        private List<string> ValidateSlides(ICollection<StorySlide> slides)
        {
            var issues = new List<string>();

            foreach (var slide in slides.OrderBy(s => s.OrderIndex))
            {
                // Check for empty slides
                if (string.IsNullOrEmpty(slide.Headline) && 
                    string.IsNullOrEmpty(slide.Text) && 
                    string.IsNullOrEmpty(slide.MediaUrl))
                {
                    issues.Add($"Slide {slide.OrderIndex + 1} is empty");
                }

                // Check duration
                if (slide.Duration < 1000)
                {
                    issues.Add($"Slide {slide.OrderIndex + 1} has very short duration ({slide.Duration}ms)");
                }

                // Check media URLs
                if (!string.IsNullOrEmpty(slide.MediaUrl))
                {
                    if (!slide.MediaUrl.StartsWith("http") && !slide.MediaUrl.StartsWith("/"))
                    {
                        issues.Add($"Slide {slide.OrderIndex + 1} has invalid media URL format");
                    }
                }
            }

            return issues;
        }

        private bool IsImageFile(string url)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            return imageExtensions.Any(ext => url.ToLower().EndsWith(ext));
        }

        private string OptimizeTitleSimple(string title)
        {
            // Simple optimization: capitalize, remove extra spaces
            return Regex.Replace(title.Trim(), @"\s+", " ");
        }

        private string GenerateDescriptionSimple(string title, string? content)
        {
            if (!string.IsNullOrEmpty(content) && content.Length > 100)
            {
                return content.Substring(0, Math.Min(160, content.Length)).Trim() + "...";
            }
            
            return $"Read {title} on DiscussionSpot. Join the conversation and share your thoughts.";
        }
    }

    public class WebStoryOptimizationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Improvements { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? OptimizedTitle { get; set; }
        public string? OptimizedMetaDescription { get; set; }
    }

    public class WebStoryValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}

