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
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            var hasTrailingSlash = path.EndsWith("/") && path.Length > 1;
            var hasQueryString = context.Request.QueryString.HasValue;

            // Remove trailing slashes (except for root "/")
            if (hasTrailingSlash)
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
                !context.Request.Host.Host.Contains("127.0.0.1"))
            {
                var httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                _logger.LogInformation($"Redirecting HTTP to HTTPS: {httpsUrl}");
                context.Response.Redirect(httpsUrl, permanent: true);
                return;
            }

            await _next(context);
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

