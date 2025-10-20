using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot9.Controllers.Api
{
    [Route("api/share")]
    [ApiController]
    public class ShareApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShareApiController> _logger;

        public ShareApiController(ApplicationDbContext context, ILogger<ShareApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Track a share event
        /// POST /api/share/track
        /// </summary>
        [HttpPost("track")]
        public async Task<IActionResult> TrackShare([FromBody] ShareTrackRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get user ID if authenticated
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get request metadata
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();
                var referrer = Request.Headers["Referer"].ToString();

                // Create share activity record
                var shareActivity = new ShareActivity
                {
                    ContentType = request.ContentType,
                    ContentId = int.Parse(request.ContentId),
                    Platform = request.Platform,
                    UserId = userId,
                    SharedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    ReferrerUrl = referrer,
                    DeviceType = GetDeviceType(userAgent),
                    BrowserName = GetBrowserName(userAgent),
                    OsName = GetOsName(userAgent)
                };

                _context.ShareActivities.Add(shareActivity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Share tracked: {ContentType} {ContentId} on {Platform}", 
                    request.ContentType, request.ContentId, request.Platform);

                return Ok(new { success = true, shareId = shareActivity.ShareId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking share");
                return StatusCode(500, new { error = "Failed to track share" });
            }
        }

        /// <summary>
        /// Get share count for content
        /// GET /api/share/count?contentType=post&contentId=123
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetShareCount([FromQuery] string contentType, [FromQuery] string contentId)
        {
            try
            {
                if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(contentId))
                {
                    return BadRequest(new { error = "ContentType and ContentId are required" });
                }

                var id = int.Parse(contentId);
                var count = await _context.ShareActivities
                    .Where(sa => sa.ContentType == contentType && sa.ContentId == id)
                    .CountAsync();

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting share count");
                return StatusCode(500, new { error = "Failed to get share count" });
            }
        }

        /// <summary>
        /// Get share analytics for content
        /// GET /api/share/analytics?contentType=post&contentId=123
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetShareAnalytics([FromQuery] string contentType, [FromQuery] string contentId, [FromQuery] int days = 30)
        {
            try
            {
                if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(contentId))
                {
                    return BadRequest(new { error = "ContentType and ContentId are required" });
                }

                var id = int.Parse(contentId);
                var startDate = DateTime.UtcNow.AddDays(-days);

                var shares = await _context.ShareActivities
                    .Where(sa => sa.ContentType == contentType && sa.ContentId == id && sa.SharedAt >= startDate)
                    .ToListAsync();

                var analytics = new
                {
                    totalShares = shares.Count,
                    byPlatform = shares.GroupBy(s => s.Platform)
                        .Select(g => new { platform = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count)
                        .ToList(),
                    byDeviceType = shares.GroupBy(s => s.DeviceType)
                        .Select(g => new { deviceType = g.Key, count = g.Count() })
                        .ToList(),
                    recentShares = shares.OrderByDescending(s => s.SharedAt)
                        .Take(10)
                        .Select(s => new
                        {
                            platform = s.Platform,
                            sharedAt = s.SharedAt,
                            userId = s.UserId
                        })
                        .ToList(),
                    sharesByDay = shares.GroupBy(s => s.SharedAt.Date)
                        .Select(g => new { date = g.Key, count = g.Count() })
                        .OrderBy(x => x.date)
                        .ToList()
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting share analytics");
                return StatusCode(500, new { error = "Failed to get share analytics" });
            }
        }

        /// <summary>
        /// Get trending shared content
        /// GET /api/share/trending?contentType=post&days=7&limit=10
        /// </summary>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingShares([FromQuery] string? contentType = null, [FromQuery] int days = 7, [FromQuery] int limit = 10)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var query = _context.ShareActivities
                    .Where(sa => sa.SharedAt >= startDate);

                if (!string.IsNullOrEmpty(contentType))
                {
                    query = query.Where(sa => sa.ContentType == contentType);
                }

                var trending = await query
                    .GroupBy(sa => new { sa.ContentType, sa.ContentId })
                    .Select(g => new
                    {
                        contentType = g.Key.ContentType,
                        contentId = g.Key.ContentId,
                        shareCount = g.Count(),
                        platforms = g.Select(s => s.Platform).Distinct().ToList()
                    })
                    .OrderByDescending(x => x.shareCount)
                    .Take(limit)
                    .ToListAsync();

                return Ok(trending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending shares");
                return StatusCode(500, new { error = "Failed to get trending shares" });
            }
        }

        // Helper methods
        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();
            if (userAgent.Contains("mobile") || userAgent.Contains("android")) return "Mobile";
            if (userAgent.Contains("tablet") || userAgent.Contains("ipad")) return "Tablet";
            return "Desktop";
        }

        private string GetBrowserName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();
            if (userAgent.Contains("edg")) return "Edge";
            if (userAgent.Contains("chrome")) return "Chrome";
            if (userAgent.Contains("firefox")) return "Firefox";
            if (userAgent.Contains("safari")) return "Safari";
            if (userAgent.Contains("opera")) return "Opera";
            return "Other";
        }

        private string GetOsName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();
            if (userAgent.Contains("windows")) return "Windows";
            if (userAgent.Contains("mac")) return "macOS";
            if (userAgent.Contains("linux")) return "Linux";
            if (userAgent.Contains("android")) return "Android";
            if (userAgent.Contains("ios") || userAgent.Contains("iphone") || userAgent.Contains("ipad")) return "iOS";
            return "Other";
        }
    }

    // Request models
    public class ShareTrackRequest
    {
        public string ContentType { get; set; } = string.Empty;
        public string ContentId { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }
}

