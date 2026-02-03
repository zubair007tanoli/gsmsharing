using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Services;

namespace Pdfpeaks.Controllers;

/// <summary>
/// Controller for sitemap and robots.txt generation
/// </summary>
public class SitemapController : Controller
{
    private readonly SitemapService _sitemapService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SitemapController> _logger;

    public SitemapController(
        SitemapService sitemapService,
        IWebHostEnvironment environment,
        ILogger<SitemapController> logger)
    {
        _sitemapService = sitemapService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Serve sitemap.xml
    /// </summary>
    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var sitemapPath = Path.Combine(_environment.WebRootPath, "sitemap.xml");
        
        // If sitemap exists and is less than 24 hours old, serve it
        if (System.IO.File.Exists(sitemapPath))
        {
            var fileInfo = new FileInfo(sitemapPath);
            if (fileInfo.LastWriteTimeUtc > DateTime.UtcNow.AddHours(-24))
            {
                return PhysicalFile(sitemapPath, "application/xml");
            }
        }

        // Regenerate sitemap
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var staticPages = SitemapService.GetStaticPages();
        var pdfTools = SitemapService.GetPdfTools();
        var imageTools = SitemapService.GetImageTools();

        var content = await _sitemapService.GenerateSitemapAsync(baseUrl, staticPages, pdfTools, imageTools);
        
        return Content(content, "application/xml");
    }

    /// <summary>
    /// Serve robots.txt
    /// </summary>
    [HttpGet("robots.txt")]
    public async Task<IActionResult> Robots()
    {
        var robotsPath = Path.Combine(_environment.WebRootPath, "robots.txt");
        
        // If robots.txt exists, serve it
        if (System.IO.File.Exists(robotsPath))
        {
            return PhysicalFile(robotsPath, "text/plain");
        }

        // Generate robots.txt
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var content = await _sitemapService.GenerateRobotsTxtAsync(baseUrl);
        
        return Content(content, "text/plain");
    }

    /// <summary>
    /// Regenerate sitemap manually (admin only)
    /// </summary>
    [HttpGet("sitemap/regenerate")]
    public async Task<IActionResult> RegenerateSitemap()
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var staticPages = SitemapService.GetStaticPages();
            var pdfTools = SitemapService.GetPdfTools();
            var imageTools = SitemapService.GetImageTools();

            await _sitemapService.GenerateSitemapAsync(baseUrl, staticPages, pdfTools, imageTools);
            
            return Ok(new { message = "Sitemap regenerated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating sitemap");
            return StatusCode(500, new { error = "Failed to regenerate sitemap" });
        }
    }
}
