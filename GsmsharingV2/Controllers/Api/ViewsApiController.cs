using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViewsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ViewsApiController> _logger;

        public ViewsApiController(
            ApplicationDbContext context,
            ILogger<ViewsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("Track/{contentType}/{contentId}")]
        public async Task<IActionResult> TrackView(string contentType, int contentId)
        {
            try
            {
                var userId = User?.Identity?.IsAuthenticated == true 
                    ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    : null;

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                if (contentType == "post")
                {
                    // Check if view already exists for this user/IP in the last hour (prevent spam)
                    var recentView = await _context.PostViews
                        .FirstOrDefaultAsync(v => v.PostID == contentId &&
                            (userId != null ? v.UserId == userId : v.IPAddress == ipAddress) &&
                            v.ViewedAt > DateTime.UtcNow.AddHours(-1));

                    if (recentView == null)
                    {
                        // Create new view record
                        var view = new PostView
                        {
                            PostID = contentId,
                            UserId = userId,
                            IPAddress = ipAddress,
                            UserAgent = userAgent,
                            ViewedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.PostViews.Add(view);

                        // Update post view count
                        var post = await _context.Posts.FindAsync(contentId);
                        if (post != null)
                        {
                            post.ViewCount = (post.ViewCount ?? 0) + 1;
                            _context.Posts.Update(post);
                        }

                        await _context.SaveChangesAsync();
                    }

                    // Return current view count
                    var postForCount = await _context.Posts.FindAsync(contentId);
                    return Ok(new
                    {
                        success = true,
                        viewCount = postForCount?.ViewCount ?? 0
                    });
                }

                return BadRequest(new { error = "Unsupported content type" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking view for {ContentType} {ContentId}", contentType, contentId);
                return StatusCode(500, new { error = "An error occurred while tracking the view" });
            }
        }
    }
}

