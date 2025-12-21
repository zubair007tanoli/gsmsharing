using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialShareController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SocialShareController> _logger;

        public SocialShareController(
            ApplicationDbContext context,
            ILogger<SocialShareController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("Track")]
        public async Task<IActionResult> TrackShare([FromBody] ShareRequest request)
        {
            try
            {
                var userId = User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
                
                var share = new SocialShare
                {
                    ContentType = request.ContentType ?? "post",
                    ContentID = request.ContentID,
                    Platform = request.Platform ?? "unknown",
                    SharedBy = userId,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.SocialShares.Add(share);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Social share tracked: {ContentType} {ContentID} on {Platform}", 
                    request.ContentType, request.ContentID, request.Platform);

                return Ok(new { success = true, message = "Share tracked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking social share");
                return StatusCode(500, new { error = "An error occurred while tracking the share" });
            }
        }
    }

    public class ShareRequest
    {
        public string? ContentType { get; set; } // "post", "comment", "forum", "blog", "product"
        public int ContentID { get; set; }
        public string? Platform { get; set; } // "facebook", "twitter", "linkedin", "whatsapp", "telegram", "email", "copy_link"
    }
}

