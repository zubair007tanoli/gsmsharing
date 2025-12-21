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
    public class CommentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentsApiController> _logger;

        public CommentsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentsApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("GetByPost/{postId}")]
        public async Task<IActionResult> GetByPost(int postId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.PostID == postId && (c.IsDeleted == false || c.IsDeleted == null))
                    .Include(c => c.User)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new
                    {
                        commentID = c.CommentID,
                        postID = c.PostID,
                        user = new
                        {
                            userId = c.UserId,
                            userName = c.User != null ? c.User.UserName : "Anonymous"
                        },
                        content = c.Content,
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt,
                        upvoteCount = c.UpvoteCount,
                        downvoteCount = c.DownvoteCount,
                        parentCommentID = c.ParentCommentID
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comments for post {PostId}", postId);
                return StatusCode(500, new { error = "An error occurred while loading comments" });
            }
        }

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateCommentRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest(new { error = "Comment content is required" });
                }

                var comment = new Comment
                {
                    PostID = request.PostID,
                    UserId = userId,
                    ParentCommentID = request.ParentCommentID > 0 ? request.ParentCommentID : null,
                    Content = request.Content.Trim(),
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                // Update post comment count
                var post = await _context.Posts.FindAsync(request.PostID);
                if (post != null)
                {
                    post.CommentCount = await _context.Comments.CountAsync(c => c.PostID == request.PostID && (c.IsDeleted == false || c.IsDeleted == null));
                    _context.Posts.Update(post);
                    await _context.SaveChangesAsync();
                }

                // Load the comment with user data
                var savedComment = await _context.Comments
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CommentID == comment.CommentID);

                return Ok(new
                {
                    success = true,
                    comment = new
                    {
                        commentID = savedComment.CommentID,
                        postID = savedComment.PostID,
                        user = new
                        {
                            userId = savedComment.UserId,
                            userName = savedComment.User?.UserName ?? "Anonymous"
                        },
                        content = savedComment.Content,
                        createdAt = savedComment.CreatedAt,
                        upvoteCount = savedComment.UpvoteCount,
                        downvoteCount = savedComment.DownvoteCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, new { error = "An error occurred while creating the comment" });
            }
        }
    }

    public class CreateCommentRequest
    {
        public int PostID { get; set; }
        public int? ParentCommentID { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}

