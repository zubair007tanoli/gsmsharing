using discussionspot9.Data.DbContext;
using discussionspot9.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace discussionspot9.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SitemapController> _logger;
        private readonly ISitemapService _sitemapService;

        public SitemapController(
            ApplicationDbContext context, 
            ILogger<SitemapController> logger,
            ISitemapService sitemapService)
        {
            _context = context;
            _logger = logger;
            _sitemapService = sitemapService;
        }

        [HttpGet]
        [Route("sitemap.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // Cache for 1 hour
        public async Task<IActionResult> Sitemap()
        {
            try
            {
                var sitemapXml = await _sitemapService.GenerateSitemapAsync(Request.Scheme, Request.Host.ToString());
                return Content(sitemapXml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap");
                return StatusCode(500, "Error generating sitemap");
            }
        }

        [HttpGet]
        [Route("sitemap-index.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> SitemapIndex()
        {
            try
            {
                var sitemapIndexXml = await _sitemapService.GenerateSitemapIndexAsync(Request.Scheme, Request.Host.ToString());
                return Content(sitemapIndexXml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap index");
                return StatusCode(500, "Error generating sitemap index");
            }
        }

    }
}