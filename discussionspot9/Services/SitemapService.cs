using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml;

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
            // Use UTF-8 StringWriter to prevent BOM issues
            using (var stringWriter = new Utf8StringWriter())
            {
                using (var xmlWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                })
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("urlset");
                    xmlWriter.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    xmlWriter.WriteAttributeString("xmlns:image", "http://www.google.com/schemas/sitemap-image/1.1");
                    xmlWriter.WriteAttributeString("xmlns:news", "http://www.google.com/schemas/sitemap-news/0.9");

                    try
                    {
                        // 1. Static Pages
                        WriteUrl(xmlWriter, $"{scheme}://{host}", DateTime.UtcNow, "daily", "1.0");
                        WriteUrl(xmlWriter, $"{scheme}://{host}/communities", DateTime.UtcNow, "daily", "0.9");
                        WriteUrl(xmlWriter, $"{scheme}://{host}/stories", DateTime.UtcNow, "daily", "0.9");
                        WriteUrl(xmlWriter, $"{scheme}://{host}/categories", DateTime.UtcNow, "daily", "0.8");

                        // 2. Communities
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

                        // 3. Posts with nested Images
                        var posts = await _context.Posts
                            .Include(p => p.Community)
                            .Include(p => p.Media)
                            .Where(p => p.Status == "published" && p.Community != null && !p.Community.IsDeleted)
                            .OrderByDescending(p => p.UpdatedAt)
                            .Take(50000)
                            .ToListAsync();

                        foreach (var post in posts)
                        {
                            var postUrl = $"{scheme}://{host}/r/{EscapeUrl(post.Community!.Slug)}/posts/{EscapeUrl(post.Slug)}";

                            // START URL ELEMENT
                            xmlWriter.WriteStartElement("url");
                            xmlWriter.WriteElementString("loc", postUrl);
                            xmlWriter.WriteElementString("lastmod", post.UpdatedAt.ToString("yyyy-MM-dd"));
                            xmlWriter.WriteElementString("changefreq", "daily");
                            xmlWriter.WriteElementString("priority", "0.8");

                            // ADD IMAGE INSIDE THE URL ELEMENT
                            var mediaUrl = post.Media?.FirstOrDefault()?.Url ?? post.Url;
                            if (!string.IsNullOrEmpty(mediaUrl))
                            {
                                var imageUrl = GetValidImageUrl(mediaUrl, scheme, host);
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    var title = post.Title ?? "";
                                    var caption = post.Content != null
                                        ? TruncateToSentence(StripHtml(post.Content), 180)
                                        : "";
                                    WriteImage(xmlWriter, imageUrl, title, caption);
                                }
                            }

                            // CLOSE URL ELEMENT
                            xmlWriter.WriteEndElement();
                        }

                        // 4. Published Stories with nested Images
                        var stories = await _context.Stories
                            .Where(s => s.Status == "published")
                            .OrderByDescending(s => s.PublishedAt ?? s.UpdatedAt)
                            .Take(10000)
                            .ToListAsync();

                        foreach (var story in stories)
                        {
                            var storyUrl = $"{scheme}://{host}/stories/viewer/{EscapeUrl(story.Slug)}";

                            // START URL ELEMENT
                            xmlWriter.WriteStartElement("url");
                            xmlWriter.WriteElementString("loc", storyUrl);
                            xmlWriter.WriteElementString("lastmod", story.UpdatedAt.ToString("yyyy-MM-dd"));
                            xmlWriter.WriteElementString("changefreq", "weekly");
                            xmlWriter.WriteElementString("priority", "0.8");

                            if (!string.IsNullOrEmpty(story.PosterImageUrl))
                            {
                                var imageUrl = GetValidImageUrl(story.PosterImageUrl, scheme, host);
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    var title = story.Title ?? "";
                                    var description = story.Description ?? "";
                                    WriteImage(xmlWriter, imageUrl, title, description);
                                }
                            }

                            // CLOSE URL ELEMENT
                            xmlWriter.WriteEndElement();

                            if (story.IsAmpEnabled)
                            {
                                var ampUrl = $"{scheme}://{host}/stories/amp/{EscapeUrl(story.Slug)}";
                                WriteUrl(xmlWriter, ampUrl, story.UpdatedAt, "weekly", "0.7");
                            }
                        }

                        // 5. Categories
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
                    }
                }

                return stringWriter.ToString();
            }
        }

        public async Task<string> GenerateSitemapIndexAsync(string scheme, string host)
        {
            using (var stringWriter = new Utf8StringWriter())
            {
                using (var xmlWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                })
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("sitemapindex");
                    xmlWriter.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    try
                    {
                        WriteSitemap(xmlWriter, $"{scheme}://{host}/sitemap.xml", DateTime.UtcNow);

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
                    }
                }

                return stringWriter.ToString();
            }
        }

        private void WriteUrl(XmlTextWriter writer, string url, DateTime lastMod, string changeFreq, string priority)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", url);
            writer.WriteElementString("lastmod", lastMod.ToString("yyyy-MM-dd"));
            writer.WriteElementString("changefreq", changeFreq);
            writer.WriteElementString("priority", priority);
            writer.WriteEndElement();
        }

        private void WriteImage(XmlTextWriter writer, string imageUrl, string title, string caption)
        {
            // This now writes to the current open element (which is 'url')
            writer.WriteStartElement("image", "image", "http://www.google.com/schemas/sitemap-image/1.1");
            writer.WriteElementString("image", "loc", "http://www.google.com/schemas/sitemap-image/1.1", imageUrl);

            if (!string.IsNullOrEmpty(title))
            {
                writer.WriteElementString("image", "title", "http://www.google.com/schemas/sitemap-image/1.1", title);
            }

            if (!string.IsNullOrEmpty(caption))
            {
                writer.WriteElementString("image", "caption", "http://www.google.com/schemas/sitemap-image/1.1", caption);
            }

            writer.WriteEndElement();
        }

        private void WriteSitemap(XmlTextWriter writer, string sitemapUrl, DateTime lastMod)
        {
            writer.WriteStartElement("sitemap");
            writer.WriteElementString("loc", sitemapUrl);
            writer.WriteElementString("lastmod", lastMod.ToString("yyyy-MM-dd"));
            writer.WriteEndElement();
        }

        private string GetValidImageUrl(string? mediaUrl, string scheme, string host)
        {
            if (string.IsNullOrEmpty(mediaUrl))
                return string.Empty;

            if (mediaUrl.StartsWith("http"))
            {
                if (mediaUrl.Contains(host))
                {
                    return mediaUrl;
                }
                return string.Empty;
            }

            return $"{scheme}://{host}{(mediaUrl.StartsWith("/") ? "" : "/")}{mediaUrl}";
        }

        private string TruncateToSentence(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            var truncated = text.Substring(0, maxLength);
            var lastPeriod = truncated.LastIndexOf('.');
            var lastQuestion = truncated.LastIndexOf('?');
            var lastExclamation = truncated.LastIndexOf('!');

            var lastSentenceEnd = Math.Max(lastPeriod, Math.Max(lastQuestion, lastExclamation));

            if (lastSentenceEnd > 50)
            {
                return text.Substring(0, lastSentenceEnd + 1);
            }

            var lastSpace = truncated.LastIndexOf(' ');
            return lastSpace > 50 ? text.Substring(0, lastSpace) + "..." : truncated + "...";
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
            text = System.Net.WebUtility.HtmlDecode(text);
            return text.Trim();
        }

        private string EscapeUrl(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
                return string.Empty;

            return Uri.EscapeDataString(slug);
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}