using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace discussionspot9.Services
{
    /// <summary>
    /// Service for generating structured data (schema.org) for images
    /// </summary>
    public class ImageStructuredDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageStructuredDataService> _logger;

        public ImageStructuredDataService(
            ApplicationDbContext context,
            ILogger<ImageStructuredDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Generate ImageObject schema for all images in a post
        /// </summary>
        public async Task<bool> GenerateImageSchemaAsync(int postId)
        {
            try
            {
                // Get post with images
                var post = await _context.Posts
                    .Include(p => p.Media)
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null || !post.Media.Any())
                {
                    return false;
                }

                // Get or create SeoMetadata
                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

                if (seoMetadata == null)
                {
                    seoMetadata = new SeoMetadata
                    {
                        EntityType = "post",
                        EntityId = postId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.SeoMetadata.Add(seoMetadata);
                }

                // Parse existing structured data
                Dictionary<string, object>? structuredData = null;
                if (!string.IsNullOrEmpty(seoMetadata.StructuredData))
                {
                    try
                    {
                        structuredData = JsonSerializer.Deserialize<Dictionary<string, object>>(seoMetadata.StructuredData);
                    }
                    catch
                    {
                        structuredData = new Dictionary<string, object>();
                    }
                }
                else
                {
                    structuredData = new Dictionary<string, object>();
                }

                // Generate image schema
                var imageSchemas = new List<object>();

                foreach (var media in post.Media)
                {
                    var imageSchema = new
                    {
                        type = "ImageObject",
                        url = media.Url,
                        contentUrl = media.Url,
                        thumbnail = media.ThumbnailUrl,
                        caption = media.Caption ?? media.AltText ?? post.Title,
                        description = media.AltText ?? $"Image for {post.Title}",
                        name = media.FileName ?? $"Image {media.MediaId}",
                        width = media.Width,
                        height = media.Height,
                        uploadDate = media.UploadedAt.ToString("yyyy-MM-dd"),
                        keywords = seoMetadata.Keywords?.Split(',').Select(k => k.Trim()).ToList() ?? new List<string>()
                    };

                    imageSchemas.Add(imageSchema);
                }

                // Add or update image schema in structured data
                structuredData["image_objects"] = imageSchemas;
                structuredData["image_count"] = imageSchemas.Count;
                structuredData["last_image_update"] = DateTime.UtcNow;

                // Save back to SeoMetadata
                seoMetadata.StructuredData = JsonSerializer.Serialize(structuredData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                seoMetadata.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Generated image schema for {Count} images in post {PostId}", 
                    imageSchemas.Count, postId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating image schema for post {PostId}", postId);
                return false;
            }
        }

        /// <summary>
        /// Generate complete Article schema with images
        /// </summary>
        public async Task<string?> GenerateArticleSchemaWithImagesAsync(int postId)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Media)
                    .Include(p => p.Community)
                    .Include(p => p.UserProfile)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return null;
                }

                var schema = new
                {
                    context = "https://schema.org",
                    type = "Article",
                    headline = post.Title,
                    description = !string.IsNullOrEmpty(post.Content) ? post.Content.Substring(0, Math.Min(200, post.Content.Length)) : "",
                    image = post.Media.Select(m => new
                    {
                        type = "ImageObject",
                        url = m.Url,
                        caption = m.Caption ?? m.AltText,
                        width = m.Width,
                        height = m.Height
                    }).ToList(),
                    datePublished = post.CreatedAt.ToString("yyyy-MM-dd"),
                    dateModified = post.UpdatedAt.ToString("yyyy-MM-dd"),
                    author = new
                    {
                        type = "Person",
                        name = post.UserProfile?.DisplayName ?? "Anonymous"
                    },
                    publisher = new
                    {
                        type = "Organization",
                        name = "DiscussionSpot",
                        logo = new
                        {
                            type = "ImageObject",
                            url = "https://discussionspot.com/images/logo.png"
                        }
                    }
                };

                return JsonSerializer.Serialize(schema, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating article schema for post {PostId}", postId);
                return null;
            }
        }
    }
}
