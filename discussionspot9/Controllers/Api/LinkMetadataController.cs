using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkMetadataController : ControllerBase
    {
        private readonly ILinkMetadataService _linkMetadataService;
        private readonly ILogger<LinkMetadataController> _logger;

        public LinkMetadataController(
            ILinkMetadataService linkMetadataService,
            ILogger<LinkMetadataController> logger)
        {
            _linkMetadataService = linkMetadataService;
            _logger = logger;
        }

        /// <summary>
        /// Get metadata for a URL (for link previews)
        /// </summary>
        /// <param name="request">Request containing the URL</param>
        /// <returns>Link metadata including title, description, image, etc.</returns>
        [HttpPost("GetMetadata")]
        public async Task<IActionResult> GetMetadata([FromBody] LinkMetadataRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    return BadRequest(new { error = "URL is required" });
                }

                // Validate URL format
                if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                {
                    return BadRequest(new { error = "Invalid URL format" });
                }

                // Only allow http and https schemes
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    return BadRequest(new { error = "Only HTTP and HTTPS URLs are supported" });
                }

                _logger.LogInformation("Fetching metadata for URL: {Url}", request.Url);

                var metadata = await _linkMetadataService.GetMetadataAsync(request.Url);

                return Ok(metadata);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP error fetching metadata for URL: {Url}", request.Url);
                
                // Return basic metadata on error
                var uri = new Uri(request.Url);
                return Ok(new LinkPreviewViewModel
                {
                    Title = uri.Host,
                    Description = "Unable to load preview",
                    Url = request.Url,
                    Domain = uri.Host,
                    FaviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching metadata for URL: {Url}", request.Url);
                return StatusCode(500, new { error = "Failed to fetch link metadata" });
            }
        }

        /// <summary>
        /// Get metadata for multiple URLs (batch request)
        /// </summary>
        /// <param name="request">Request containing multiple URLs</param>
        /// <returns>Array of link metadata</returns>
        [HttpPost("GetMetadataBatch")]
        public async Task<IActionResult> GetMetadataBatch([FromBody] LinkMetadataBatchRequest request)
        {
            try
            {
                if (request.Urls == null || !request.Urls.Any())
                {
                    return BadRequest(new { error = "At least one URL is required" });
                }

                // Limit batch size to prevent abuse
                if (request.Urls.Count > 10)
                {
                    return BadRequest(new { error = "Maximum 10 URLs per batch request" });
                }

                var results = new List<LinkPreviewViewModel>();

                foreach (var url in request.Urls)
                {
                    try
                    {
                        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                        {
                            var metadata = await _linkMetadataService.GetMetadataAsync(url);
                            results.Add(metadata);
                        }
                        else
                        {
                            // Add basic metadata for invalid URLs
                            results.Add(new LinkPreviewViewModel
                            {
                                Url = url,
                                Title = "Invalid URL",
                                Description = "Unable to load preview",
                                Domain = ""
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching metadata for URL in batch: {Url}", url);
                        
                        // Add error metadata
                        var uri = new Uri(url);
                        results.Add(new LinkPreviewViewModel
                        {
                            Url = url,
                            Title = uri.Host,
                            Description = "Unable to load preview",
                            Domain = uri.Host,
                            FaviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico"
                        });
                    }
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch metadata request");
                return StatusCode(500, new { error = "Failed to fetch link metadata" });
            }
        }
    }

    /// <summary>
    /// Request model for single URL metadata
    /// </summary>
    public class LinkMetadataRequest
    {
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for batch URL metadata
    /// </summary>
    public class LinkMetadataBatchRequest
    {
        public List<string> Urls { get; set; } = new();
    }
}

