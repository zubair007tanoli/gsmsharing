using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Services;
using System.Text.Json;

namespace Pdfpeaks.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken] // API uses key authentication, not CSRF
    public class McpController : ControllerBase
    {
        private readonly SEOMcpServer _seoMcpServer;
        private readonly PerformanceMcpServer _performanceMcpServer;

        public McpController(SEOMcpServer seoMcpServer, PerformanceMcpServer performanceMcpServer)
        {
            _seoMcpServer = seoMcpServer;
            _performanceMcpServer = performanceMcpServer;
        }

        #region SEO Endpoints

        [HttpGet("seo/metadata/{**path}")]
        public IActionResult GetSeoMetadata(string path)
        {
            var metadata = _seoMcpServer.GetMetadata("/" + path);
            if (metadata == null)
            {
                return NotFound(new { error = "No metadata found for this path" });
            }
            return Ok(metadata);
        }

        [HttpGet("seo/analyze/{**path}")]
        public IActionResult AnalyzeSeo(string path)
        {
            var result = _seoMcpServer.AnalyzePage("/" + path);
            return Ok(result);
        }

        [HttpGet("seo/robots")]
        public IActionResult GetRobotsTxt()
        {
            var robotsTxt = _seoMcpServer.GenerateRobotsTxt();
            return Content(robotsTxt, "text/plain");
        }

        [HttpGet("seo/sitemap")]
        public IActionResult GetSitemapXml()
        {
            var sitemap = _seoMcpServer.GenerateSitemapXml();
            return Content(sitemap, "text/xml");
        }

        [HttpGet("seo/paths")]
        public IActionResult GetAllPaths()
        {
            var paths = _seoMcpServer.GetAllRegisteredPaths();
            return Ok(new { paths });
        }

        #endregion

        #region Performance Endpoints

        [HttpGet("performance/stats")]
        public IActionResult GetPerformanceStats()
        {
            var stats = _performanceMcpServer.GetStatistics();
            return Ok(stats);
        }

        [HttpGet("performance/endpoints")]
        public IActionResult GetEndpointPerformance()
        {
            var performance = _performanceMcpServer.GetEndpointPerformance();
            return Ok(performance);
        }

        [HttpGet("performance/alerts")]
        public IActionResult GetAlerts([FromQuery] string? level = null)
        {
            AlertLevel? alertLevel = null;
            if (!string.IsNullOrEmpty(level) && Enum.TryParse<AlertLevel>(level, true, out var parsed))
            {
                alertLevel = parsed;
            }
            var alerts = _performanceMcpServer.GetAlerts(alertLevel);
            return Ok(alerts);
        }

        [HttpDelete("performance/alerts")]
        public IActionResult ClearAlerts()
        {
            _performanceMcpServer.ClearAlerts();
            return Ok(new { message = "Alerts cleared" });
        }

        [HttpGet("performance/cache")]
        public IActionResult GetCacheStats()
        {
            var stats = _performanceMcpServer.GetCacheStats();
            return Ok(stats);
        }

        [HttpGet("performance/health")]
        public IActionResult GetHealthCheck()
        {
            var health = _performanceMcpServer.PerformHealthCheck();
            return Ok(health);
        }

        [HttpGet("performance/optimizations")]
        public IActionResult GetOptimizations()
        {
            var suggestions = _performanceMcpServer.GetOptimizationSuggestions();
            return Ok(suggestions);
        }

        #endregion

        #region Combined Dashboard

        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok(new
            {
                seo = new
                {
                    totalPaths = _seoMcpServer.GetAllRegisteredPaths().Count,
                    score = CalculateOverallSeoScore()
                },
                performance = _performanceMcpServer.GetStatistics(),
                health = _performanceMcpServer.PerformHealthCheck(),
                suggestions = _performanceMcpServer.GetOptimizationSuggestions()
            });
        }

        private double CalculateOverallSeoScore()
        {
            var paths = _seoMcpServer.GetAllRegisteredPaths();
            if (!paths.Any()) return 0;

            var totalScore = paths.Sum(p => _seoMcpServer.AnalyzePage(p).Percentage);
            return Math.Round((double)(totalScore / paths.Count), 2);
        }

        #endregion
    }
}
