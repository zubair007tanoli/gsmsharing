using discussionspot9.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace discussionspot9.Controllers
{
    [ApiController] // Add this for cleaner API behavior
    [Route("")]
    public class SitemapController : ControllerBase // Use ControllerBase instead of Controller
    {
        private readonly ILogger<SitemapController> _logger;
        private readonly ISitemapService _sitemapService;

        public SitemapController(
            ILogger<SitemapController> logger,
            ISitemapService sitemapService)
        {
            _logger = logger;
            _sitemapService = sitemapService;
        }

        [HttpGet("sitemap.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        [Produces("application/xml")]
        public async Task<IActionResult> Sitemap()
        {
            try
            {
                var scheme = Request.Scheme;
                var host = Request.Host.ToString();

                var sitemapXml = await _sitemapService.GenerateSitemapAsync(scheme, host);

                // Ensure no BOM or extra whitespace
                sitemapXml = sitemapXml.Trim();

                // Return with explicit charset
                return Content(sitemapXml, "application/xml; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap");
                return StatusCode(500, "Error generating sitemap");
            }
        }

        [HttpGet("sitemap-index.xml")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        [Produces("application/xml")]
        public async Task<IActionResult> SitemapIndex()
        {
            try
            {
                var scheme = Request.Scheme;
                var host = Request.Host.ToString();

                var sitemapIndexXml = await _sitemapService.GenerateSitemapIndexAsync(scheme, host);

                sitemapIndexXml = sitemapIndexXml.Trim();

                return Content(sitemapIndexXml, "application/xml; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap index");
                return StatusCode(500, "Error generating sitemap index");
            }
        }

        [HttpGet("robots.txt")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        [Produces("text/plain")]
        public IActionResult RobotsTxt()
        {
            var scheme = Request.Scheme;
            var host = Request.Host.ToString();

            var robotsTxt = new StringBuilder();
            robotsTxt.AppendLine("User-agent: *");
            robotsTxt.AppendLine("Allow: /");
            robotsTxt.AppendLine();
            robotsTxt.AppendLine($"Sitemap: {scheme}://{host}/sitemap.xml");

            return Content(robotsTxt.ToString(), "text/plain; charset=utf-8");
        }
    }
}