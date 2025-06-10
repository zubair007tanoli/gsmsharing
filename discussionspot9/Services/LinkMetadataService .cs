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

            // Set a user agent to avoid being blocked by some sites
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<LinkPreviewViewModel> GetMetadataAsync(string url)
        {
            var metadata = new LinkPreviewViewModel { Url = url };

            try
            {
                // Extract domain
                var uri = new Uri(url);
                metadata.Domain = uri.Host;

                // Fetch HTML content
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Extract title
                metadata.Title = ExtractTitle(doc);

                // Extract description
                metadata.Description = ExtractDescription(doc);

                // Extract thumbnail/image
                metadata.ThumbnailUrl = ExtractThumbnail(doc, uri);

                // Extract favicon
                metadata.FaviconUrl = ExtractFavicon(doc, uri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from {Url}", url);
            }

            return metadata;
        }

        private string ExtractTitle(HtmlDocument doc)
        {
            // Try Open Graph title first
            var ogTitle = doc.DocumentNode
                .SelectSingleNode("//meta[@property='og:title']")
                ?.GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(ogTitle))
                return ogTitle;

            // Try Twitter title
            var twitterTitle = doc.DocumentNode
                .SelectSingleNode("//meta[@name='twitter:title']")
                ?.GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(twitterTitle))
                return twitterTitle;

            // Fallback to HTML title tag
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            return titleNode?.InnerText?.Trim() ?? "";
        }

        private string ExtractDescription(HtmlDocument doc)
        {
            // Try Open Graph description first
            var ogDescription = doc.DocumentNode
                .SelectSingleNode("//meta[@property='og:description']")
                ?.GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(ogDescription))
                return ogDescription;

            // Try Twitter description
            var twitterDescription = doc.DocumentNode
                .SelectSingleNode("//meta[@name='twitter:description']")
                ?.GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(twitterDescription))
                return twitterDescription;

            // Try standard meta description
            var metaDescription = doc.DocumentNode
                .SelectSingleNode("//meta[@name='description']")
                ?.GetAttributeValue("content", "");

            return metaDescription ?? "";
        }

        private string ExtractThumbnail(HtmlDocument doc, Uri baseUri)
        {
            // Try Open Graph image first
            var ogImage = doc.DocumentNode
                .SelectSingleNode("//meta[@property='og:image']")
                ?.GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(ogImage))
                return MakeAbsoluteUrl(ogImage, baseUri);

            // Try Twitter image
            var twitterImage = doc.DocumentNode
                .SelectSingleNode("//meta[@name='twitter:image']")
                ?.GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(twitterImage))
                return MakeAbsoluteUrl(twitterImage, baseUri);

            // Try to find first meaningful image
            var imgNode = doc.DocumentNode
                .SelectSingleNode("//img[@src]");

            if (imgNode != null)
            {
                var src = imgNode.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(src))
                    return MakeAbsoluteUrl(src, baseUri);
            }

            return "";
        }

        private string ExtractFavicon(HtmlDocument doc, Uri baseUri)
        {
            // Try to find favicon link tags
            var faviconSelectors = new[]
            {
            "//link[@rel='icon']",
            "//link[@rel='shortcut icon']",
            "//link[@rel='apple-touch-icon']"
        };

            foreach (var selector in faviconSelectors)
            {
                var faviconNode = doc.DocumentNode.SelectSingleNode(selector);
                if (faviconNode != null)
                {
                    var href = faviconNode.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(href))
                        return MakeAbsoluteUrl(href, baseUri);
                }
            }

            // Default favicon location
            return $"{baseUri.Scheme}://{baseUri.Host}/favicon.ico";
        }

        private string MakeAbsoluteUrl(string url, Uri baseUri)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            if (Uri.TryCreate(baseUri, url, out var absoluteUri))
                return absoluteUri.ToString();

            return url;
        }
    }
}
