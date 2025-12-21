using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GsmsharingV2.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;
        private readonly IHubContext<CommentHub> _commentHub;

        public CommentsController(
            ICommentService commentService, 
            ILogger<CommentsController> logger,
            IHubContext<CommentHub> commentHub)
        {
            _commentService = commentService;
            _logger = logger;
            _commentHub = commentHub;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByPost(int postId)
        {
            var comments = await _commentService.GetByPostIdAsync(postId);
            return Json(comments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var comment = await _commentService.CreateAsync(createCommentDto, userId);
                
                // Send real-time notification via SignalR
                if (createCommentDto.PostID > 0)
                {
                    var userName = User.Identity?.Name ?? "Anonymous";
                    await _commentHub.Clients.Group($"post-{createCommentDto.PostID}")
                        .SendAsync("ReceiveComment", new
                        {
                            CommentID = comment.CommentID,
                            PostID = createCommentDto.PostID,
                            Content = comment.Content,
                            UserName = userName,
                            CreatedAt = comment.CreatedAt
                        });
                }
                
                return Json(new { success = true, comment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, new { success = false, message = "Error creating comment" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var comment = await _commentService.UpdateAsync(updateCommentDto, userId);
                return Json(new { success = true, comment });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment");
                return StatusCode(500, new { success = false, message = "Error updating comment" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _commentService.DeleteAsync(id, userId);
                return Json(new { success = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                return StatusCode(500, new { success = false, message = "Error deleting comment" });
            }
        }
    }
}

