using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using HtmlAgilityPack;

namespace discussionspot9.Services
{
    public class LinkMetadataService : ILinkMetadataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LinkMetadataService> _logger;

        public LinkMetadataService(HttpClient httpClient, ILogger<LinkMetadataService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Set a more complete user agent string
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // Add timeout
        }

        public async Task<LinkPreviewViewModel> GetMetadataAsync(string url)
        {
            var metadata = new LinkPreviewViewModel { Url = url };

            try
            {
                var uri = new Uri(url);
                metadata.Domain = uri.Host;

                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Check if content is HTML
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (!contentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    metadata.Title = "Non-HTML Content";
                    metadata.Description = $"Content type: {contentType}";
                    return metadata;
                }

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Extract metadata in order of preference
                metadata.Title = ExtractMetadata(doc, new[]
                {
                    "//meta[@property='og:title']/@content",
                    "//meta[@name='twitter:title']/@content",
                    "//title/text()",
                    "//h1/text()"
                }) ?? uri.Host;

                metadata.Description = ExtractMetadata(doc, new[]
                {
                    "//meta[@property='og:description']/@content",
                    "//meta[@name='twitter:description']/@content",
                    "//meta[@name='description']/@content",
                    "//meta[@property='description']/@content"
                }) ?? "";

                metadata.ThumbnailUrl = ExtractMetadata(doc, new[]
                {
                    "//meta[@property='og:image']/@content",
                    "//meta[@name='twitter:image']/@content",
                    "//link[@rel='image_src']/@href",
                    "//img[@id='main-image']/@src",
                    "//img[contains(@class, 'hero')]/@src"
                });

                if (string.IsNullOrEmpty(metadata.ThumbnailUrl))
                {
                    // Find first meaningful image (skip tiny images and icons)
                    var imgs = doc.DocumentNode.SelectNodes("//img[@src]")?
                        .Select(img => img.GetAttributeValue("src", ""))
                        .Where(src => !src.Contains("icon", StringComparison.OrdinalIgnoreCase) 
                                  && !src.Contains("logo", StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();
                    
                    if (!string.IsNullOrEmpty(imgs))
                    {
                        metadata.ThumbnailUrl = MakeAbsoluteUrl(imgs, uri);
                    }
                }

                metadata.FaviconUrl = ExtractFavicon(doc, uri);

                // Clean up and validate URLs
                metadata.ThumbnailUrl = CleanUrl(metadata.ThumbnailUrl, uri);
                metadata.FaviconUrl = CleanUrl(metadata.FaviconUrl, uri);

                // Trim metadata
                metadata.Title = TrimMetadata(metadata.Title, 200);
                metadata.Description = TrimMetadata(metadata.Description, 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from {Url}", url);
                metadata.Title = metadata.Domain;
                metadata.Description = "Unable to load preview";
            }

            return metadata;
        }

        private string? ExtractMetadata(HtmlDocument doc, string[] xpaths)
        {
            foreach (var xpath in xpaths)
            {
                var node = doc.DocumentNode.SelectSingleNode(xpath);
                var value = node?.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            return null;
        }

        private string ExtractFavicon(HtmlDocument doc, Uri baseUri)
        {
            var faviconUrl = ExtractMetadata(doc, new[]
            {
                "//link[@rel='icon' and contains(@sizes, '32')]/@href",
                "//link[@rel='shortcut icon']/@href",
                "//link[@rel='icon']/@href",
                "//link[@rel='apple-touch-icon']/@href"
            });

            if (string.IsNullOrEmpty(faviconUrl))
            {
                return $"{baseUri.Scheme}://{baseUri.Host}/favicon.ico";
            }

            return MakeAbsoluteUrl(faviconUrl, baseUri);
        }

        private string MakeAbsoluteUrl(string url, Uri baseUri)
        {
            if (string.IsNullOrEmpty(url)) return "";
            
            // Handle data URLs
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return url;

            // Remove whitespace and invalid characters
            url = url.Trim().Replace(" ", "%20");

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            if (url.StartsWith("//"))
                return $"{baseUri.Scheme}:{url}";

            if (Uri.TryCreate(baseUri, url, out var absoluteUri))
                return absoluteUri.ToString();

            return "";
        }

        private string CleanUrl(string url, Uri baseUri)
        {
            if (string.IsNullOrEmpty(url)) return "";
            try
            {
                return MakeAbsoluteUrl(url, baseUri);
            }
            catch
            {
                return "";
            }
        }

        private string TrimMetadata(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            text = text.Trim();
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }
    }
}
