using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReportsApiController> _logger;

        public ReportsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReportsApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("Post")]
        public async Task<IActionResult> ReportPost([FromForm] ReportPostRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // Check if user already reported this post
                var existingReport = await _context.PostReports
                    .FirstOrDefaultAsync(r => r.PostID == request.PostID && r.ReportedBy == userId);

                if (existingReport != null)
                {
                    return BadRequest(new { error = "You have already reported this post" });
                }

                var report = new PostReport
                {
                    PostID = request.PostID,
                    ReportedBy = userId,
                    Reason = request.Reason ?? "Other",
                    Details = request.Details,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PostReports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Post {PostID} reported by user {UserId} for reason: {Reason}", 
                    request.PostID, userId, request.Reason);

                return Ok(new { success = true, message = "Report submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting post report");
                return StatusCode(500, new { error = "An error occurred while submitting the report" });
            }
        }

        [HttpPost("Comment")]
        public async Task<IActionResult> ReportComment([FromForm] ReportCommentRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // Check if user already reported this comment
                var existingReport = await _context.CommentReports
                    .FirstOrDefaultAsync(r => r.CommentID == request.CommentID && r.ReportedBy == userId);

                if (existingReport != null)
                {
                    return BadRequest(new { error = "You have already reported this comment" });
                }

                var report = new CommentReport
                {
                    CommentID = request.CommentID,
                    ReportedBy = userId,
                    Reason = request.Reason ?? "Other",
                    Details = request.Details,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.CommentReports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentID} reported by user {UserId} for reason: {Reason}", 
                    request.CommentID, userId, request.Reason);

                return Ok(new { success = true, message = "Report submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting comment report");
                return StatusCode(500, new { error = "An error occurred while submitting the report" });
            }
        }
    }

    public class ReportPostRequest
    {
        public int PostID { get; set; }
        public string? Reason { get; set; }
        public string? Details { get; set; }
    }

    public class ReportCommentRequest
    {
        public int CommentID { get; set; }
        public string? Reason { get; set; }
        public string? Details { get; set; }
    }
}

