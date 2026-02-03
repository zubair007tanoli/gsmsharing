using Microsoft.EntityFrameworkCore;
using Pdfpeaks.Models;

namespace Pdfpeaks.Services;

/// <summary>
/// Service for automatic sitemap generation
/// </summary>
public class SitemapService
{
    private readonly ILogger<SitemapService> _logger;
    private readonly IWebHostEnvironment _environment;

    public SitemapService(ILogger<SitemapService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Generate sitemap.xml file
    /// </summary>
    public async Task<string> GenerateSitemapAsync(
        string baseUrl, 
        List<string> staticPages, 
        List<string> pdfTools, 
        List<string> imageTools)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Add static pages
        foreach (var page in staticPages)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}{page}</loc>");
            sb.AppendLine($"    <changefreq>weekly</changefreq>");
            sb.AppendLine($"    <priority>{GetPriority(page)}</priority>");
            sb.AppendLine("  </url>");
        }

        // Add PDF tools
        foreach (var tool in pdfTools)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/Pdf/{tool}</loc>");
            sb.AppendLine($"    <changefreq>weekly</changefreq>");
            sb.AppendLine($"    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        // Add Image tools
        foreach (var tool in imageTools)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/Image/{tool}</loc>");
            sb.AppendLine($"    <changefreq>weekly</changefreq>");
            sb.AppendLine($"    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        // Add robots.txt location
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/robots.txt</loc>");
        sb.AppendLine($"    <changefreq>monthly</changefreq>");
        sb.AppendLine($"    <priority>0.3</priority>");
        sb.AppendLine("  </url>");

        sb.AppendLine("</urlset>");

        var sitemapContent = sb.ToString();
        
        // Save sitemap file
        var sitemapPath = Path.Combine(_environment.WebRootPath, "sitemap.xml");
        await File.WriteAllTextAsync(sitemapPath, sitemapContent);
        
        _logger.LogInformation("Sitemap generated at {Path}", sitemapPath);
        
        return sitemapContent;
    }

    /// <summary>
    /// Generate robots.txt file
    /// </summary>
    public async Task<string> GenerateRobotsTxtAsync(string baseUrl)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        sb.AppendLine();
        sb.AppendLine("# Disallow admin areas");
        sb.AppendLine("Disallow: /Admin/");
        sb.AppendLine("Disallow: /UserAccounts/");
        sb.AppendLine("Disallow: /Api/");
        sb.AppendLine();
        sb.AppendLine("# Disallow sensitive directories");
        sb.AppendLine("Disallow: /temp/");
        sb.AppendLine("Disallow: /logs/");

        var robotsContent = sb.ToString();
        
        // Save robots.txt file
        var robotsPath = Path.Combine(_environment.WebRootPath, "robots.txt");
        await File.WriteAllTextAsync(robotsPath, robotsContent);
        
        _logger.LogInformation("Robots.txt generated at {Path}", robotsPath);
        
        return robotsContent;
    }

    /// <summary>
    /// Get static pages for sitemap
    /// </summary>
    public static List<string> GetStaticPages()
    {
        return new List<string>
        {
            "/",
            "/Home/Index",
            "/Home/Contact",
            "/Home/Privacy",
            "/UserAccounts/Login",
            "/UserAccounts/Register",
            "/Pdf/Index",
            "/Image/Index"
        };
    }

    /// <summary>
    /// Get PDF tools for sitemap
    /// </summary>
    public static List<string> GetPdfTools()
    {
        return new List<string>
        {
            "Merge",
            "Split",
            "Compress",
            "ConvertToWord",
            "ConvertToExcel",
            "ConvertToPowerPoint",
            "ConvertFromWord",
            "ConvertFromExcel",
            "ConvertFromPowerPoint",
            "ConvertToJpg",
            "ConvertFromJpg",
            "ConvertToPng",
            "Organize",
            "Edit",
            "Rotate",
            "Unlock",
            "Protect"
        };
    }

    /// <summary>
    /// Get Image tools for sitemap
    /// </summary>
    public static List<string> GetImageTools()
    {
        return new List<string>
        {
            "Resize",
            "Crop",
            "Rotate",
            "Flip",
            "Convert",
            "Compress",
            "ToPdf",
            "Edit",
            "ToJpg",
            "ToPng",
            "ToWebp",
            "ToGif",
            "ToBmp",
            "ToIco",
            "FromPdf"
        };
    }

    /// <summary>
    /// Get priority based on URL
    /// </summary>
    private static double GetPriority(string url)
    {
        return url switch
        {
            "/" or "/Home/Index" => 1.0,
            "/Home/Contact" => 0.6,
            "/Home/Privacy" => 0.5,
            "/UserAccounts/Login" => 0.4,
            "/UserAccounts/Register" => 0.4,
            "/Pdf/Index" => 0.9,
            "/Image/Index" => 0.9,
            _ => 0.7
        };
    }
}
