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
    public class VoteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<VoteController> _logger;

        public VoteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<VoteController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("Vote")]
        public async Task<IActionResult> Vote([FromBody] VoteRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                if (request.VoteType != 1 && request.VoteType != -1)
                {
                    return BadRequest(new { error = "Invalid vote type. Must be 1 (upvote) or -1 (downvote)" });
                }

                if (request.ContentType == "post")
                {
                    return await HandlePostVote(userId, request.ContentID, request.VoteType);
                }
                else if (request.ContentType == "comment")
                {
                    return await HandleCommentVote(userId, request.ContentID, request.VoteType);
                }
                else
                {
                    return BadRequest(new { error = "Invalid content type. Must be 'post' or 'comment'" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing vote");
                return StatusCode(500, new { error = "An error occurred while processing your vote" });
            }
        }

        private async Task<IActionResult> HandlePostVote(string userId, int postId, int voteType)
        {
            var existingVote = await _context.PostVotes
                .FirstOrDefaultAsync(v => v.PostID == postId && v.UserId == userId);

            if (existingVote != null)
            {
                // If clicking the same vote type, remove the vote (toggle off)
                if (existingVote.VoteType == voteType)
                {
                    _context.PostVotes.Remove(existingVote);
                    voteType = 0; // No vote
                }
                else
                {
                    // Change vote type
                    existingVote.VoteType = voteType;
                    existingVote.UpdatedAt = DateTime.UtcNow;
                    _context.PostVotes.Update(existingVote);
                }
            }
            else
            {
                // Create new vote
                var newVote = new PostVote
                {
                    PostID = postId,
                    UserId = userId,
                    VoteType = voteType,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PostVotes.Add(newVote);
            }

            await _context.SaveChangesAsync();

            // Recalculate vote counts
            var upvotes = await _context.PostVotes.CountAsync(v => v.PostID == postId && v.VoteType == 1);
            var downvotes = await _context.PostVotes.CountAsync(v => v.PostID == postId && v.VoteType == -1);

            // Update post vote counts
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.UpvoteCount = upvotes;
                post.DownvoteCount = downvotes;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }

            // Get user's current vote state
            var userVote = voteType;
            if (voteType == 0)
            {
                userVote = 0;
            }

            return Ok(new
            {
                success = true,
                upvotes = upvotes,
                downvotes = downvotes,
                userVote = userVote
            });
        }

        private async Task<IActionResult> HandleCommentVote(string userId, int commentId, int voteType)
        {
            var existingVote = await _context.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentID == commentId && v.UserId == userId);

            if (existingVote != null)
            {
                // If clicking the same vote type, remove the vote (toggle off)
                if (existingVote.VoteType == voteType)
                {
                    _context.CommentVotes.Remove(existingVote);
                    voteType = 0; // No vote
                }
                else
                {
                    // Change vote type
                    existingVote.VoteType = voteType;
                    existingVote.UpdatedAt = DateTime.UtcNow;
                    _context.CommentVotes.Update(existingVote);
                }
            }
            else
            {
                // Create new vote
                var newVote = new CommentVote
                {
                    CommentID = commentId,
                    UserId = userId,
                    VoteType = voteType,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CommentVotes.Add(newVote);
            }

            await _context.SaveChangesAsync();

            // Recalculate vote counts
            var upvotes = await _context.CommentVotes.CountAsync(v => v.CommentID == commentId && v.VoteType == 1);
            var downvotes = await _context.CommentVotes.CountAsync(v => v.CommentID == commentId && v.VoteType == -1);

            // Update comment vote counts
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.UpvoteCount = upvotes;
                comment.DownvoteCount = downvotes;
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
            }

            // Get user's current vote state
            var userVote = voteType;
            if (voteType == 0)
            {
                userVote = 0;
            }

            return Ok(new
            {
                success = true,
                upvotes = upvotes,
                downvotes = downvotes,
                userVote = userVote
            });
        }
    }

    public class VoteRequest
    {
        public string ContentType { get; set; } = string.Empty; // "post" or "comment"
        public int ContentID { get; set; }
        public int VoteType { get; set; } // 1 = upvote, -1 = downvote
    }
}

