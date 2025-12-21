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
    public class SavedPostsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SavedPostsApiController> _logger;

        public SavedPostsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SavedPostsApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SavePost([FromBody] SavePostRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // Check if post exists
                var post = await _context.Posts.FindAsync(request.PostID);
                if (post == null)
                {
                    return NotFound(new { error = "Post not found" });
                }

                // Check if already saved
                var existingSave = await _context.SavedPosts
                    .FirstOrDefaultAsync(sp => sp.PostID == request.PostID && sp.UserId == userId);

                if (existingSave != null)
                {
                    // Unsave (remove from saved)
                    _context.SavedPosts.Remove(existingSave);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, saved = false, message = "Post unsaved" });
                }

                // Save the post
                var savedPost = new SavedPost
                {
                    PostID = request.PostID,
                    UserId = userId,
                    SavedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SavedPosts.Add(savedPost);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, saved = true, message = "Post saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving post");
                return StatusCode(500, new { error = "An error occurred while saving the post" });
            }
        }

        [HttpPost("Check/{postId}")]
        [Authorize]
        public async Task<IActionResult> CheckSaved(int postId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new { saved = false });
                }

                var isSaved = await _context.SavedPosts
                    .AnyAsync(sp => sp.PostID == postId && sp.UserId == userId);

                return Ok(new { saved = isSaved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking saved status");
                return Ok(new { saved = false });
            }
        }
    }

    public class SavePostRequest
    {
        public int PostID { get; set; }
    }
}

