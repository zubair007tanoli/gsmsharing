using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GsmsharingV2.Middleware
{
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCachingMiddleware> _logger;

        public ResponseCachingMiddleware(RequestDelegate next, ILogger<ResponseCachingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add cache headers for static assets
            if (context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/lib") ||
                context.Request.Path.StartsWithSegments("/images"))
            {
                context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
            }
            // Cache API responses for a shorter time
            else if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Headers.Append("Cache-Control", "public, max-age=60");
            }
            // No cache for dynamic pages
            else
            {
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                context.Response.Headers.Append("Pragma", "no-cache");
                context.Response.Headers.Append("Expires", "0");
            }

            await _next(context);
        }
    }
}
