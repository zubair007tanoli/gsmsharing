using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.GoogleSearch;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace discussionspot9.Services
{
    /// <summary>
    /// Service for optimizing image SEO using Semrush keyword data
    /// </summary>
    public class ImageSeoOptimizer
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleSearchService _googleSearchService;
        private readonly ILogger<ImageSeoOptimizer> _logger;

        public ImageSeoOptimizer(
            ApplicationDbContext context,
            GoogleSearchService googleSearchService,
            ILogger<ImageSeoOptimizer> logger)
        {
            _context = context;
            _googleSearchService = googleSearchService;
            _logger = logger;
        }

        /// <summary>
        /// Optimize all images for a post with Semrush keywords
        /// </summary>
        public async Task<ImageOptimizationResult> OptimizePostImagesAsync(int postId)
        {
            try
            {
                // Get post with images
                var post = await _context.Posts
                    .Include(p => p.Media)
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new ImageOptimizationResult
                    {
                        Success = false,
                        ErrorMessage = "Post not found"
                    };
                }

                // Get Google Search keywords for the post
                var keywords = await GetPostGoogleKeywordsAsync(postId);

                if (keywords.Count == 0)
                {
                    // If no Google data, extract keywords from post
                    keywords = ExtractKeywordsFromPost(post);
                }

                var result = new ImageOptimizationResult
                {
                    Success = true,
                    PostId = postId,
                    TotalImages = post.Media.Count,
                    Keywords = keywords
                };

                // Optimize each image
                foreach (var media in post.Media)
                {
                    try
                    {
                        var optimized = await OptimizeImageAsync(media, keywords, post);
                        
                        if (optimized)
                        {
                            result.ImagesOptimized++;
                            result.OptimizedImages.Add(new ImageOptimizationDetail
                            {
                                MediaId = media.MediaId,
                                OriginalAltText = media.AltText,
                                OptimizedAltText = media.AltText,
                                OriginalCaption = media.Caption,
                                OptimizedCaption = media.Caption,
                                OriginalFileName = media.FileName,
                                OptimizedFileName = media.FileName
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error optimizing image {MediaId}", media.MediaId);
                        result.FailedImages++;
                    }
                }

                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing images for post {PostId}", postId);
                return new ImageOptimizationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Optimize a single image with SEO best practices
        /// </summary>
        private async Task<bool> OptimizeImageAsync(Media media, List<string> keywords, Post post)
        {
            bool wasOptimized = false;

            // 1. Optimize Alt Text
            if (string.IsNullOrEmpty(media.AltText) || media.AltText.Length < 10)
            {
                media.AltText = GenerateAltText(keywords, post.Title, media);
                wasOptimized = true;
            }

            // 2. Optimize Caption
            if (string.IsNullOrEmpty(media.Caption))
            {
                media.Caption = GenerateCaption(keywords, post.Title, media);
                wasOptimized = true;
            }

            // 3. Suggest SEO-friendly filename (don't rename existing files, just store suggestion)
            if (!string.IsNullOrEmpty(media.FileName))
            {
                var seoFilename = GenerateSeoFilename(keywords, post.Slug, media.MediaId);
                // Store in caption or metadata for reference
                if (!media.Caption.Contains("SEO filename:"))
                {
                    media.Caption = media.Caption + $" [SEO filename: {seoFilename}]";
                }
            }

            return wasOptimized;
        }

        /// <summary>
        /// Generate SEO-optimized alt text
        /// </summary>
        private string GenerateAltText(List<string> keywords, string postTitle, Media media)
        {
            // Strategy: Use primary keyword + descriptive text
            var primaryKeyword = keywords.FirstOrDefault() ?? "image";
            var altText = $"{primaryKeyword} - {TruncateText(postTitle, 50)}";

            // Add media type context
            if (media.MediaType == "image")
            {
                altText = $"{primaryKeyword} image showing {TruncateText(postTitle, 40)}";
            }

            // Limit to 125 characters (SEO best practice)
            return TruncateText(altText, 125);
        }

        /// <summary>
        /// Generate SEO-optimized caption
        /// </summary>
        private string GenerateCaption(List<string> keywords, string postTitle, Media media)
        {
            // Strategy: Use 2-3 keywords in natural sentence
            var keywordPhrase = string.Join(", ", keywords.Take(3));
            var caption = $"Illustration of {keywordPhrase} related to {TruncateText(postTitle, 60)}";

            return caption;
        }

        /// <summary>
        /// Generate SEO-friendly filename
        /// </summary>
        private string GenerateSeoFilename(List<string> keywords, string postSlug, int mediaId)
        {
            // Strategy: primary-keyword-post-slug-id.extension
            var primaryKeyword = keywords.FirstOrDefault()?.Replace(" ", "-") ?? "image";
            var slugPart = TruncateText(postSlug, 30).Replace(" ", "-");
            
            // Remove special characters
            primaryKeyword = Regex.Replace(primaryKeyword, @"[^a-zA-Z0-9-]", "");
            slugPart = Regex.Replace(slugPart, @"[^a-zA-Z0-9-]", "");

            return $"{primaryKeyword}-{slugPart}-{mediaId}";
        }

        /// <summary>
        /// Get Google Search keywords for a post from SeoMetadata
        /// </summary>
        private async Task<List<string>> GetPostGoogleKeywordsAsync(int postId)
        {
            var seoMetadata = await _context.SeoMetadata
                .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

            if (seoMetadata == null || string.IsNullOrEmpty(seoMetadata.StructuredData))
            {
                return new List<string>();
            }

            try
            {
                var structuredData = JsonSerializer.Deserialize<Dictionary<string, object>>(seoMetadata.StructuredData);
                
                if (structuredData != null && structuredData.ContainsKey("recommended_keywords"))
                {
                    var keywordsJson = structuredData["recommended_keywords"].ToString();
                    return JsonSerializer.Deserialize<List<string>>(keywordsJson ?? "[]") ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing Google keywords from metadata for post {PostId}", postId);
            }

            // Fallback to Keywords field
            if (!string.IsNullOrEmpty(seoMetadata.Keywords))
            {
                return seoMetadata.Keywords.Split(',').Select(k => k.Trim()).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Extract keywords from post content
        /// </summary>
        private List<string> ExtractKeywordsFromPost(Post post)
        {
            var text = $"{post.Title} {post.Content}".ToLower();
            var words = text.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']' }, 
                                   StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 4)
                .ToList();

            var stopWords = new HashSet<string>
            {
                "this", "that", "these", "those", "what", "which", "who", "when",
                "where", "why", "how", "with", "from", "have", "been", "will",
                "would", "could", "should", "your", "their", "about"
            };

            return words
                .Where(w => !stopWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }

        /// <summary>
        /// Batch optimize all images without alt text
        /// </summary>
        public async Task<BatchImageOptimizationResult> BatchOptimizeImagesWithoutAltTextAsync(int limit = 100)
        {
            try
            {
                // Get images without alt text
                var imagesWithoutAlt = await _context.Media
                    .Where(m => m.PostId != null && (m.AltText == null || m.AltText.Length < 10))
                    .Take(limit)
                    .ToListAsync();

                var result = new BatchImageOptimizationResult
                {
                    TotalImagesProcessed = imagesWithoutAlt.Count
                };

                foreach (var media in imagesWithoutAlt)
                {
                    try
                    {
                        if (media.PostId.HasValue)
                        {
                            var postResult = await OptimizePostImagesAsync(media.PostId.Value);
                            
                            if (postResult.Success)
                            {
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.FailureCount++;
                            }

                            // Rate limiting
                            await Task.Delay(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error optimizing image {MediaId}", media.MediaId);
                        result.FailureCount++;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch image optimization");
                return new BatchImageOptimizationResult
                {
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get image SEO status for dashboard
        /// </summary>
        public async Task<ImageSeoStatus> GetImageSeoStatusAsync()
        {
            var totalImages = await _context.Media.CountAsync(m => m.PostId != null);
            var imagesWithAlt = await _context.Media.CountAsync(m => m.PostId != null && !string.IsNullOrEmpty(m.AltText) && m.AltText.Length >= 10);
            var imagesWithCaption = await _context.Media.CountAsync(m => m.PostId != null && !string.IsNullOrEmpty(m.Caption));
            var imagesWithoutAlt = totalImages - imagesWithAlt;

            return new ImageSeoStatus
            {
                TotalImages = totalImages,
                ImagesWithAltText = imagesWithAlt,
                ImagesWithoutAltText = imagesWithoutAlt,
                ImagesWithCaption = imagesWithCaption,
                OptimizationPercentage = totalImages > 0 ? (decimal)imagesWithAlt / totalImages * 100 : 0
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }

    /// <summary>
    /// Result of image optimization
    /// </summary>
    public class ImageOptimizationResult
    {
        public bool Success { get; set; }
        public int PostId { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalImages { get; set; }
        public int ImagesOptimized { get; set; }
        public int FailedImages { get; set; }
        public List<string> Keywords { get; set; } = new();
        public List<ImageOptimizationDetail> OptimizedImages { get; set; } = new();
    }

    /// <summary>
    /// Details of individual image optimization
    /// </summary>
    public class ImageOptimizationDetail
    {
        public int MediaId { get; set; }
        public string? OriginalAltText { get; set; }
        public string? OptimizedAltText { get; set; }
        public string? OriginalCaption { get; set; }
        public string? OptimizedCaption { get; set; }
        public string? OriginalFileName { get; set; }
        public string? OptimizedFileName { get; set; }
    }

    /// <summary>
    /// Batch optimization result
    /// </summary>
    public class BatchImageOptimizationResult
    {
        public int TotalImagesProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Overall image SEO status
    /// </summary>
    public class ImageSeoStatus
    {
        public int TotalImages { get; set; }
        public int ImagesWithAltText { get; set; }
        public int ImagesWithoutAltText { get; set; }
        public int ImagesWithCaption { get; set; }
        public decimal OptimizationPercentage { get; set; }
    }
}
