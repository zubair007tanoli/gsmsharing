using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
        public async Task<IActionResult> Sitemap()
        {
            var sitemap = new StringBuilder();
            sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
            sitemap.AppendLine("        xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\"");
            sitemap.AppendLine("        xmlns:news=\"http://www.google.com/schemas/sitemap-news/0.9\">");

            // Home page
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{Request.Scheme}://{Request.Host}</loc>");
            sitemap.AppendLine("    <changefreq>daily</changefreq>");
            sitemap.AppendLine("    <priority>1.0</priority>");
            sitemap.AppendLine("  </url>");

            // Stories index
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{Request.Scheme}://{Request.Host}/stories</loc>");
            sitemap.AppendLine("    <changefreq>daily</changefreq>");
            sitemap.AppendLine("    <priority>0.9</priority>");
            sitemap.AppendLine("  </url>");

            // Individual stories
            var stories = await _context.Stories
                .Where(s => s.Status == "published")
                .OrderByDescending(s => s.PublishedAt)
                .Take(1000) // Limit to 1000 most recent stories
                .ToListAsync();

            foreach (var story in stories)
            {
                sitemap.AppendLine("  <url>");
                sitemap.AppendLine($"    <loc>{Request.Scheme}://{Request.Host}/stories/{story.Slug}</loc>");
                sitemap.AppendLine($"    <lastmod>{story.UpdatedAt:yyyy-MM-dd}</lastmod>");
                sitemap.AppendLine("    <changefreq>weekly</changefreq>");
                sitemap.AppendLine("    <priority>0.8</priority>");
                
                if (!string.IsNullOrEmpty(story.PosterImageUrl))
                {
                    sitemap.AppendLine("    <image:image>");
                    sitemap.AppendLine($"      <image:loc>{story.PosterImageUrl}</image:loc>");
                    sitemap.AppendLine($"      <image:title>{story.Title}</image:title>");
                    sitemap.AppendLine($"      <image:caption>{story.Description}</image:caption>");
                    sitemap.AppendLine("    </image:image>");
                }
                
                sitemap.AppendLine("  </url>");
            }

            // AMP Stories
            var ampStories = stories.Where(s => s.IsAmpEnabled).Take(100);
            foreach (var story in ampStories)
            {
                sitemap.AppendLine("  <url>");
                sitemap.AppendLine($"    <loc>{Request.Scheme}://{Request.Host}/stories/amp/{story.Slug}</loc>");
                sitemap.AppendLine($"    <lastmod>{story.UpdatedAt:yyyy-MM-dd}</lastmod>");
                sitemap.AppendLine("    <changefreq>weekly</changefreq>");
                sitemap.AppendLine("    <priority>0.7</priority>");
                sitemap.AppendLine("  </url>");
            }

            sitemap.AppendLine("</urlset>");

            return Content(sitemap.ToString(), "application/xml");
        }

        [HttpGet]
        [Route("sitemap-stories.xml")]
        public async Task<IActionResult> StoriesSitemap()
        {
            var sitemap = new StringBuilder();
            sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            var stories = await _context.Stories
                .Where(s => s.Status == "published")
                .OrderByDescending(s => s.PublishedAt)
                .ToListAsync();

            foreach (var story in stories)
            {
                sitemap.AppendLine("  <url>");
                sitemap.AppendLine($"    <loc>{Request.Scheme}://{Request.Host}/stories/{story.Slug}</loc>");
                sitemap.AppendLine($"    <lastmod>{story.UpdatedAt:yyyy-MM-dd}</lastmod>");
                sitemap.AppendLine("    <changefreq>weekly</changefreq>");
                sitemap.AppendLine("    <priority>0.8</priority>");
                sitemap.AppendLine("  </url>");
            }

            sitemap.AppendLine("</urlset>");

            return Content(sitemap.ToString(), "application/xml");
        }
    }
}