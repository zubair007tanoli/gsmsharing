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
    public class ReactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReactionsController> _logger;

        public ReactionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReactionsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddReaction([FromBody] AddReactionRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // Check if user already reacted
                var existingReaction = await _context.Reactions
                    .FirstOrDefaultAsync(r => 
                        r.UserId == userId && 
                        r.PostID == request.PostID && 
                        r.CommentID == null);

                if (existingReaction != null)
                {
                    // If same reaction, remove it (toggle)
                    if (existingReaction.ReactionType == request.ReactionType)
                    {
                        _context.Reactions.Remove(existingReaction);
                    }
                    else
                    {
                        // Change reaction type
                        existingReaction.ReactionType = request.ReactionType;
                        existingReaction.CreatedAt = DateTime.UtcNow;
                        _context.Reactions.Update(existingReaction);
                    }
                }
                else
                {
                    // Create new reaction
                    var reaction = new Reaction
                    {
                        UserId = userId,
                        PostID = request.PostID,
                        ReactionType = request.ReactionType,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Reactions.Add(reaction);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction");
                return StatusCode(500, new { error = "An error occurred while adding reaction" });
            }
        }

        [HttpGet("Get")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReactions(int? postID, int? commentID)
        {
            try
            {
                var query = _context.Reactions.AsQueryable();

                if (postID.HasValue)
                {
                    query = query.Where(r => r.PostID == postID);
                }
                else if (commentID.HasValue)
                {
                    query = query.Where(r => r.CommentID == commentID);
                }
                else
                {
                    return BadRequest(new { error = "Either postID or commentID must be provided" });
                }

                var reactions = await query
                    .GroupBy(r => r.ReactionType)
                    .Select(g => new
                    {
                        reactionType = g.Key,
                        count = g.Count()
                    })
                    .ToListAsync();

                return Ok(reactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reactions");
                return StatusCode(500, new { error = "An error occurred while getting reactions" });
            }
        }
    }

    public class AddReactionRequest
    {
        public int? PostID { get; set; }
        public int? CommentID { get; set; }
        public string ReactionType { get; set; } = string.Empty; // "like", "love", "laugh", "wow", "sad", "angry"
    }
}
