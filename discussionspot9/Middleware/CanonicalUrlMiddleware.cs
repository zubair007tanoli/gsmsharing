using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace discussionspot9.Middleware
{
    /// <summary>
    /// Middleware to enforce canonical URLs and prevent duplicate content issues in GSC
    /// </summary>
    public class CanonicalUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CanonicalUrlMiddleware> _logger;

        public CanonicalUrlMiddleware(RequestDelegate next, ILogger<CanonicalUrlMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Don't process redirects for error pages or static files
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
                
                // FIXED: Exclude AMP routes from canonical URL processing to prevent interference with AMP page serving
                if (path.StartsWith("/home/error") || 
                    path.StartsWith("/error") ||
                    path.StartsWith("/stories/amp/") ||  // Exclude AMP story routes
                    path.Contains(".css") || 
                    path.Contains(".js") || 
                    path.Contains(".ico") ||
                    context.Response.HasStarted)
                {
                    await _next(context);
                    return;
                }

                var hasTrailingSlash = path.EndsWith("/") && path.Length > 1;

                // Remove trailing slashes (except for root "/")
                if (hasTrailingSlash && !context.Response.HasStarted)
                {
                    var newPath = path.TrimEnd('/');
                    var newUrl = newPath + context.Request.QueryString;
                    
                    _logger.LogInformation($"Redirecting {path} to {newPath} (removing trailing slash)");
                    context.Response.Redirect(newUrl, permanent: true);
                    return;
                }

                // Enforce HTTPS in production
                if (!context.Request.IsHttps && 
                    !context.Request.Host.Host.Contains("localhost") &&
                    !context.Request.Host.Host.Contains("127.0.0.1") &&
                    !context.Response.HasStarted)
                {
                    var httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                    _logger.LogInformation($"Redirecting HTTP to HTTPS: {httpsUrl}");
                    context.Response.Redirect(httpsUrl, permanent: true);
                    return;
                }

                // Enforce non-www (remove www prefix) - prevents duplicate content in Google
                var host = context.Request.Host.Host;
                if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) && 
                    !host.Contains("localhost") &&
                    !context.Response.HasStarted)
                {
                    var newHost = host.Substring(4); // Remove "www."
                    var newUrl = $"https://{newHost}{context.Request.Path}{context.Request.QueryString}";
                    _logger.LogInformation($"Redirecting www to non-www: {newUrl}");
                    context.Response.Redirect(newUrl, permanent: true);
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent infinite loops
                _logger.LogError(ex, "Error in CanonicalUrlMiddleware");
                await _next(context);
            }
        }
    }

    public static class CanonicalUrlMiddlewareExtensions
    {
        public static IApplicationBuilder UseCanonicalUrls(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CanonicalUrlMiddleware>();
        }
    }
}

