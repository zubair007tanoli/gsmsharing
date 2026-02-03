using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pdfpeaks.Services
{
    /// <summary>
    /// SEO MCP Server - Provides SEO analysis and optimization capabilities
    /// for Pdfpeaks PDF tools platform
    /// </summary>
    public class SEOMcpServer
    {
        private readonly Dictionary<string, SEOMetadata> _pageMetadata = new();

        public SEOMcpServer()
        {
            InitializeDefaultMetadata();
        }

        private void InitializeDefaultMetadata()
        {
            // Home Page
            RegisterPage("/home", new SEOMetadata
            {
                Title = "PdfPeaks - Online PDF Converter, Editor & Viewer",
                Description = "Free online PDF tools to convert, edit, merge, split, compress, and protect PDFs. No installation required. Fast and secure.",
                Keywords = "pdf tools, pdf converter, pdf editor, merge pdf, split pdf, compress pdf, pdf to word, pdf to excel",
                CanonicalUrl = "https://pdfpeaks.com/home",
                OgTitle = "PdfPeaks - All-in-One PDF Solution",
                OgDescription = "Convert, edit, and manage your PDFs online with our powerful tools",
                OgImage = "/images/og-home.jpg",
                TwitterCard = "summary_large_image"
            });

            // Login Page
            RegisterPage("/useraccounts/login", new SEOMetadata
            {
                Title = "Login - PdfPeaks",
                Description = "Sign in to your PdfPeaks account to access premium features",
                Keywords = "login, sign in, pdf tools account",
                CanonicalUrl = "https://pdfpeaks.com/useraccounts/login",
                NoIndex = true
            });

            // Register Page
            RegisterPage("/useraccounts/register", new SEOMetadata
            {
                Title = "Register - PdfPeaks",
                Description = "Create a free account to start using PdfPeaks",
                Keywords = "register, sign up, create account, pdf tools",
                CanonicalUrl = "https://pdfpeaks.com/useraccounts/register",
                NoIndex = true
            });

            // Dashboard
            RegisterPage("/useraccounts/dashboard", new SEOMetadata
            {
                Title = "Dashboard - PdfPeaks",
                Description = "Your PdfPeaks dashboard - manage conversions, history, and settings",
                Keywords = "dashboard, pdf dashboard, conversion history",
                CanonicalUrl = "https://pdfpeaks.com/useraccounts/dashboard",
                NoIndex = true
            });

            // Profile
            RegisterPage("/useraccounts/profile", new SEOMetadata
            {
                Title = "My Profile - PdfPeaks",
                Description = "Manage your PdfPeaks profile settings",
                Keywords = "profile, account settings, user profile",
                CanonicalUrl = "https://pdfpeaks.com/useraccounts/profile",
                NoIndex = true
            });

            // Settings
            RegisterPage("/useraccounts/settings", new SEOMetadata
            {
                Title = "Settings - PdfPeaks",
                Description = "Configure your PdfPeaks account settings",
                Keywords = "settings, account settings, preferences",
                CanonicalUrl = "https://pdfpeaks.com/useraccounts/settings",
                NoIndex = true
            });
        }

        public void RegisterPage(string path, SEOMetadata metadata)
        {
            _pageMetadata[path.ToLower()] = metadata;
        }

        public SEOMetadata? GetMetadata(string path)
        {
            var normalizedPath = path.ToLower();
            
            // Try exact match first
            if (_pageMetadata.TryGetValue(normalizedPath, out var metadata))
                return metadata;

            // Try to find matching pattern
            foreach (var kvp in _pageMetadata)
            {
                if (normalizedPath.StartsWith(kvp.Key))
                    return kvp.Value;
            }

            return null;
        }

        public List<string> GetAllRegisteredPaths()
        {
            return _pageMetadata.Keys.ToList();
        }

        public SEOAnalysisResult AnalyzePage(string path)
        {
            var metadata = GetMetadata(path);
            var result = new SEOAnalysisResult
            {
                Path = path,
                Score = 0,
                MaxScore = 100
            };

            if (metadata == null)
            {
                result.Issues.Add("No metadata found for this page");
                return result;
            }

            // Title analysis
            if (!string.IsNullOrEmpty(metadata.Title))
            {
                result.Score += 20;
                if (metadata.Title.Length >= 50 && metadata.Title.Length <= 60)
                    result.Score += 5;
            }
            else
            {
                result.Issues.Add("Missing page title");
            }

            // Description analysis
            if (!string.IsNullOrEmpty(metadata.Description))
            {
                result.Score += 20;
                if (metadata.Description.Length >= 150 && metadata.Description.Length <= 160)
                    result.Score += 5;
            }
            else
            {
                result.Issues.Add("Missing meta description");
            }

            // Keywords analysis
            if (!string.IsNullOrEmpty(metadata.Keywords))
            {
                result.Score += 15;
                var keywordCount = metadata.Keywords.Split(',').Length;
                if (keywordCount >= 5 && keywordCount <= 10)
                    result.Score += 5;
            }

            // Canonical URL
            if (!string.IsNullOrEmpty(metadata.CanonicalUrl))
            {
                result.Score += 15;
            }
            else
            {
                result.Issues.Add("Missing canonical URL");
            }

            // Open Graph
            if (!string.IsNullOrEmpty(metadata.OgTitle) && !string.IsNullOrEmpty(metadata.OgDescription))
            {
                result.Score += 10;
            }

            // NoIndex check
            if (!metadata.NoIndex)
            {
                result.Score += 10;
            }

            // Structured Data
            if (!string.IsNullOrEmpty(metadata.SchemaType))
            {
                result.Score += 10;
            }

            result.Percentage = (result.Score * 100) / result.MaxScore;
            return result;
        }

        public string GenerateRobotsTxt()
        {
            return @"# Robots.txt for PdfPeaks
User-agent: *
Allow: /

# Sitemaps
Sitemap: https://pdfpeaks.com/sitemap.xml

# Disallow private/admin areas
Disallow: /useraccounts/
Disallow: /admin/
Disallow: /api/

# Allow specific tools
Allow: /tools/
Allow: /home/
Allow: /privacy/
Allow: /terms/
";
        }

        public string GenerateSitemapXml()
        {
            var urls = new List<string>
            {
                "https://pdfpeaks.com/home",
                "https://pdfpeaks.com/tools",
                "https://pdfpeaks.com/privacy",
                "https://pdfpeaks.com/terms"
            };

            foreach (var path in _pageMetadata.Keys)
            {
                var metadata = _pageMetadata[path];
                if (!metadata.NoIndex && !string.IsNullOrEmpty(metadata.CanonicalUrl))
                {
                    urls.Add(metadata.CanonicalUrl);
                }
            }

            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">";

            foreach (var url in urls.Distinct())
            {
                xml += $@"
    <url>
        <loc>{url}</loc>
        <changefreq>weekly</changefreq>
        <priority>0.8</priority>
    </url>";
            }

            xml += @"
</urlset>";

            return xml;
        }
    }

    public class SEOMetadata
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Keywords { get; set; } = "";
        public string CanonicalUrl { get; set; } = "";
        public string OgTitle { get; set; } = "";
        public string OgDescription { get; set; } = "";
        public string OgImage { get; set; } = "";
        public string TwitterCard { get; set; } = "summary";
        public bool NoIndex { get; set; }
        public string SchemaType { get; set; } = "";
        public string Author { get; set; } = "";
        public string PublishedTime { get; set; } = "";
        public string ModifiedTime { get; set; } = "";
    }

    public class SEOAnalysisResult
    {
        public string Path { get; set; } = "";
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int Percentage { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }
}
