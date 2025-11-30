using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml;
using System.Web;

namespace discussionspot9.Services
{
    public interface ISitemapService
    {
        Task<string> GenerateSitemapAsync(string scheme, string host);
        Task<string> GenerateSitemapIndexAsync(string scheme, string host);
    }

    public class SitemapService : ISitemapService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SitemapService> _logger;

        public SitemapService(ApplicationDbContext context, ILogger<SitemapService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateSitemapAsync(string scheme, string host)
        {
            var sitemap = new StringBuilder();
            var writer = new StringWriter(sitemap);
            var xmlWriter = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("urlset");
            xmlWriter.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            xmlWriter.WriteAttributeString("xmlns:image", "http://www.google.com/schemas/sitemap-image/1.1");
            xmlWriter.WriteAttributeString("xmlns:news", "http://www.google.com/schemas/sitemap-news/0.9");

            try
            {
                // Home page
                WriteUrl(xmlWriter, $"{scheme}://{host}", DateTime.UtcNow, "daily", "1.0");

                // Communities index
                WriteUrl(xmlWriter, $"{scheme}://{host}/communities", DateTime.UtcNow, "daily", "0.9");

                // Stories index
                WriteUrl(xmlWriter, $"{scheme}://{host}/stories", DateTime.UtcNow, "daily", "0.9");

                // Categories index
                WriteUrl(xmlWriter, $"{scheme}://{host}/categories", DateTime.UtcNow, "daily", "0.8");

                // Communities
                var communities = await _context.Communities
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10000)
                    .ToListAsync();

                foreach (var community in communities)
                {
                    var url = $"{scheme}://{host}/r/{EscapeUrl(community.Slug)}";
                    WriteUrl(xmlWriter, url, community.UpdatedAt, "daily", "0.9");
                }

                // Posts (most important for SEO!)
                var posts = await _context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.Media)
                    .Where(p => p.Status == "published" && p.Community != null && !p.Community.IsDeleted)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Take(50000) // Google allows up to 50,000 URLs per sitemap
                    .ToListAsync();

                foreach (var post in posts)
                {
                    var url = $"{scheme}://{host}/r/{EscapeUrl(post.Community!.Slug)}/posts/{EscapeUrl(post.Slug)}";
                    WriteUrl(xmlWriter, url, post.UpdatedAt, "daily", "0.8");

                    // Add post image if available (from Media collection or Url field)
                    var mediaUrl = post.Media?.FirstOrDefault()?.Url ?? post.Url;
                    if (!string.IsNullOrEmpty(mediaUrl))
                    {
                        var imageUrl = mediaUrl.StartsWith("http") 
                            ? mediaUrl 
                            : $"{scheme}://{host}{mediaUrl}";
                        var title = EscapeXml(post.Title ?? "");
                        var caption = post.Content != null 
                            ? EscapeXml(post.Content.Substring(0, Math.Min(200, post.Content.Length))) 
                            : "";
                        WriteImage(xmlWriter, imageUrl, title, caption);
                    }
                }

                // Published Stories
                var stories = await _context.Stories
                    .Where(s => s.Status == "published")
                    .OrderByDescending(s => s.PublishedAt ?? s.UpdatedAt)
                    .Take(10000)
                    .ToListAsync();

                foreach (var story in stories)
                {
                    // Regular story viewer
                    var storyUrl = $"{scheme}://{host}/stories/viewer/{EscapeUrl(story.Slug)}";
                    WriteUrl(xmlWriter, storyUrl, story.UpdatedAt, "weekly", "0.8");

                    // Add story image
                    if (!string.IsNullOrEmpty(story.PosterImageUrl))
                    {
                        var imageUrl = story.PosterImageUrl.StartsWith("http") 
                            ? story.PosterImageUrl 
                            : $"{scheme}://{host}{story.PosterImageUrl}";
                        WriteImage(xmlWriter, imageUrl, story.Title ?? "", story.Description ?? "");
                    }

                    // AMP story if enabled
                    if (story.IsAmpEnabled)
                    {
                        var ampUrl = $"{scheme}://{host}/stories/amp/{EscapeUrl(story.Slug)}";
                        WriteUrl(xmlWriter, ampUrl, story.UpdatedAt, "weekly", "0.7");
                    }
                }

                // Categories
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(1000)
                    .ToListAsync();

                foreach (var category in categories)
                {
                    var url = $"{scheme}://{host}/categories/{EscapeUrl(category.Slug)}";
                    WriteUrl(xmlWriter, url, category.UpdatedAt, "weekly", "0.7");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap");
            }
            finally
            {
                xmlWriter.WriteEndElement(); // urlset
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }

            return sitemap.ToString();
        }

        public async Task<string> GenerateSitemapIndexAsync(string scheme, string host)
        {
            var sitemap = new StringBuilder();
            var writer = new StringWriter(sitemap);
            var xmlWriter = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("sitemapindex");
            xmlWriter.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

            try
            {
                // Main sitemap
                WriteSitemap(xmlWriter, $"{scheme}://{host}/sitemap.xml", DateTime.UtcNow);

                // Posts sitemap (if you have >50k posts, split into multiple)
                var postCount = await _context.Posts
                    .Where(p => p.Status == "published")
                    .CountAsync();

                if (postCount > 50000)
                {
                    var sitemapCount = (int)Math.Ceiling(postCount / 50000.0);
                    for (int i = 1; i <= sitemapCount; i++)
                    {
                        WriteSitemap(xmlWriter, $"{scheme}://{host}/sitemap-posts-{i}.xml", DateTime.UtcNow);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap index");
            }
            finally
            {
                xmlWriter.WriteEndElement(); // sitemapindex
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }

            return sitemap.ToString();
        }

        private void WriteUrl(XmlTextWriter writer, string url, DateTime lastMod, string changeFreq, string priority)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", EscapeXml(url));
            writer.WriteElementString("lastmod", lastMod.ToString("yyyy-MM-dd"));
            writer.WriteElementString("changefreq", changeFreq);
            writer.WriteElementString("priority", priority);
            writer.WriteEndElement();
        }

        private void WriteImage(XmlTextWriter writer, string imageUrl, string title, string caption)
        {
            writer.WriteStartElement("image", "image", "http://www.google.com/schemas/sitemap-image/1.1");
            writer.WriteElementString("image", "loc", "http://www.google.com/schemas/sitemap-image/1.1", EscapeXml(imageUrl));
            
            if (!string.IsNullOrEmpty(title))
            {
                writer.WriteElementString("image", "title", "http://www.google.com/schemas/sitemap-image/1.1", EscapeXml(title));
            }
            
            if (!string.IsNullOrEmpty(caption))
            {
                writer.WriteElementString("image", "caption", "http://www.google.com/schemas/sitemap-image/1.1", EscapeXml(caption));
            }
            
            writer.WriteEndElement();
        }

        private void WriteSitemap(XmlTextWriter writer, string sitemapUrl, DateTime lastMod)
        {
            writer.WriteStartElement("sitemap");
            writer.WriteElementString("loc", EscapeXml(sitemapUrl));
            writer.WriteElementString("lastmod", lastMod.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();
        }

        private string EscapeXml(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private string EscapeUrl(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
                return string.Empty;

            // URL encode the slug
            return Uri.EscapeDataString(slug);
        }
    }
}

