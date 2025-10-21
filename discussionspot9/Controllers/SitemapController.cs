using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml;

namespace discussionspot9.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SitemapController> _logger;

        public SitemapController(ApplicationDbContext context, ILogger<SitemapController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("sitemap.xml")]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> Index()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var sitemap = await GenerateSitemapXmlAsync(baseUrl);
                
                return Content(sitemap, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap");
                return StatusCode(500);
            }
        }

        private async Task<string> GenerateSitemapXmlAsync(string baseUrl)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Homepage
            AddUrl(sb, baseUrl, DateTime.UtcNow, "daily", "1.0");

            // Static pages
            AddUrl(sb, $"{baseUrl}/communities", DateTime.UtcNow, "daily", "0.9");
            AddUrl(sb, $"{baseUrl}/popular", DateTime.UtcNow, "daily", "0.8");
            AddUrl(sb, $"{baseUrl}/categories", DateTime.UtcNow, "weekly", "0.8");

            // Categories
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new { c.Slug, c.UpdatedAt })
                .AsNoTracking()
                .ToListAsync();

            foreach (var category in categories)
            {
                AddUrl(sb, $"{baseUrl}/category/{category.Slug}", category.UpdatedAt, "weekly", "0.7");
            }

            // Communities
            var communities = await _context.Communities
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Slug, c.UpdatedAt })
                .AsNoTracking()
                .ToListAsync();

            foreach (var community in communities)
            {
                AddUrl(sb, $"{baseUrl}/r/{community.Slug}", community.UpdatedAt, "daily", "0.8");
            }

            // Posts (published only, exclude deleted/removed content)
            // IMPROVED: Better filtering to prevent 404 errors in GSC
            var posts = await _context.Posts
                .Where(p => p.Status == "published" 
                         && !p.Community.IsDeleted
                         && p.Community != null
                         && !string.IsNullOrEmpty(p.Slug)
                         && !string.IsNullOrEmpty(p.Community.Slug))
                .OrderByDescending(p => p.UpdatedAt)
                .Take(10000)
                .Select(p => new 
                { 
                    CommunitySlug = p.Community.Slug, 
                    p.Slug, 
                    p.UpdatedAt,
                    p.ViewCount
                })
                .AsNoTracking()
                .ToListAsync();

            foreach (var post in posts)
            {
                // Higher priority for popular posts
                var priority = post.ViewCount > 1000 ? "0.9" : 
                              post.ViewCount > 500 ? "0.8" : 
                              post.ViewCount > 100 ? "0.7" : "0.6";
                
                var changefreq = post.ViewCount > 500 ? "daily" : "weekly";
                
                AddUrl(sb, $"{baseUrl}/r/{post.CommunitySlug}/posts/{post.Slug}", 
                      post.UpdatedAt, changefreq, priority);
            }

            // User profiles (active users with posts)
            var activeUsers = await _context.UserProfiles
                .Where(u => _context.Posts.Any(p => p.UserId == u.UserId && p.Status == "published"))
                .Select(u => new { u.DisplayName, u.LastActive })
                .Take(5000)
                .AsNoTracking()
                .ToListAsync();

            foreach (var user in activeUsers)
            {
                AddUrl(sb, $"{baseUrl}/u/{user.DisplayName}", user.LastActive, "weekly", "0.5");
            }

            sb.AppendLine("</urlset>");
            return sb.ToString();
        }

        private void AddUrl(StringBuilder sb, string loc, DateTime lastmod, string changefreq, string priority)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{XmlEscape(loc)}</loc>");
            sb.AppendLine($"    <lastmod>{lastmod:yyyy-MM-dd}</lastmod>");
            sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            sb.AppendLine("  </url>");
        }

        private string XmlEscape(string text)
        {
            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&apos;");
        }
    }
}

