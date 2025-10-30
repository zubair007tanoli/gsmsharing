using discussionspot9.Models.Domain;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services
{
    /// <summary>
    /// Service for optimizing AMP Stories performance
    /// </summary>
    public class StoryPerformanceOptimizer
    {
        private readonly ILogger<StoryPerformanceOptimizer> _logger;

        public StoryPerformanceOptimizer(ILogger<StoryPerformanceOptimizer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Optimize images for AMP Stories (resize, compress, convert to WebP)
        /// </summary>
        public async Task<string> OptimizeImageForAMP(string imageUrl, int maxWidth = 828, int maxHeight = 1792)
        {
            try
            {
                // In production, implement actual image optimization
                // For now, return optimized URL suggestion
                // This would typically:
                // 1. Download image
                // 2. Resize to maxWidth x maxHeight
                // 3. Convert to WebP if supported
                // 4. Compress with quality optimization
                // 5. Upload to CDN
                // 6. Return optimized URL

                _logger.LogInformation($"Optimizing image for AMP: {imageUrl}");
                
                // For now, return original URL
                // TODO: Implement actual image optimization pipeline
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error optimizing image: {imageUrl}");
                return imageUrl; // Return original on error
            }
        }

        /// <summary>
        /// Validate AMP Story for performance best practices
        /// </summary>
        public StoryPerformanceReport ValidateStoryPerformance(Story story, IEnumerable<StorySlide> slides)
        {
            var report = new StoryPerformanceReport
            {
                StoryId = story.StoryId,
                TotalSlides = slides.Count(),
                TotalDuration = slides.Sum(s => s.Duration)
            };

            // Check slide count (optimal: 5-15 slides)
            if (report.TotalSlides < 5)
            {
                report.Warnings.Add("Story has fewer than 5 slides. Consider adding more content for better engagement.");
            }
            else if (report.TotalSlides > 20)
            {
                report.Warnings.Add("Story has more than 20 slides. Consider breaking into multiple stories.");
            }

            // Check total duration (optimal: 30-60 seconds)
            var totalSeconds = report.TotalDuration / 1000;
            if (totalSeconds < 20)
            {
                report.Warnings.Add("Story duration is less than 20 seconds. Consider adding more content.");
            }
            else if (totalSeconds > 90)
            {
                report.Warnings.Add("Story duration exceeds 90 seconds. Users may lose interest.");
            }

            // Check for images without alt text
            var imagesWithoutAlt = slides.Where(s => 
                s.SlideType == "image" && 
                string.IsNullOrEmpty(s.Caption) && 
                string.IsNullOrEmpty(s.Text)
            ).Count();
            
            if (imagesWithoutAlt > 0)
            {
                report.Warnings.Add($"{imagesWithoutAlt} image(s) missing alt text or captions. This affects accessibility and SEO.");
            }

            // Check for videos without posters
            var videosWithoutPosters = slides.Where(s => 
                s.SlideType == "video" && 
                string.IsNullOrEmpty(s.MediaUrl)
            ).Count();
            
            if (videosWithoutPosters > 0)
            {
                report.Warnings.Add($"{videosWithoutPosters} video(s) missing poster images. This affects loading performance.");
            }

            // Performance score calculation
            report.PerformanceScore = CalculatePerformanceScore(report);
            
            return report;
        }

        private int CalculatePerformanceScore(StoryPerformanceReport report)
        {
            var score = 100;
            
            // Deduct points for warnings
            score -= report.Warnings.Count * 10;
            
            // Deduct points for suboptimal slide count
            if (report.TotalSlides < 5 || report.TotalSlides > 20)
            {
                score -= 15;
            }
            
            // Deduct points for suboptimal duration
            var totalSeconds = report.TotalDuration / 1000;
            if (totalSeconds < 20 || totalSeconds > 90)
            {
                score -= 15;
            }
            
            return Math.Max(0, score);
        }

        /// <summary>
        /// Generate preload hints for AMP Story
        /// </summary>
        public List<string> GeneratePreloadHints(IEnumerable<StorySlide> slides)
        {
            var hints = new List<string>();
            
            // Preload first 3 slides' media
            var firstSlides = slides.OrderBy(s => s.OrderIndex).Take(3);
            
            foreach (var slide in firstSlides)
            {
                if (!string.IsNullOrEmpty(slide.MediaUrl))
                {
                    if (IsImageUrl(slide.MediaUrl))
                    {
                        hints.Add($"<link rel=\"preload\" as=\"image\" href=\"{slide.MediaUrl}\">");
                    }
                    else if (IsVideoUrl(slide.MediaUrl))
                    {
                        hints.Add($"<link rel=\"preload\" as=\"video\" href=\"{slide.MediaUrl}\">");
                    }
                }
            }
            
            return hints;
        }

        private bool IsImageUrl(string url)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            return imageExtensions.Any(ext => url.ToLower().EndsWith(ext));
        }

        private bool IsVideoUrl(string url)
        {
            var videoExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov" };
            return videoExtensions.Any(ext => url.ToLower().EndsWith(ext));
        }
    }

    public class StoryPerformanceReport
    {
        public int StoryId { get; set; }
        public int TotalSlides { get; set; }
        public int TotalDuration { get; set; }
        public int PerformanceScore { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }
}

