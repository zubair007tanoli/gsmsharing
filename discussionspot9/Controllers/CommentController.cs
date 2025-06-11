using discussionspot9.Hubs;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Internal;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly ILogger<CommentController> _logger;
        private readonly IHubContext<PostHub> _postHub;

        public CommentController(
            ICommentService commentService,
            IUserService userService,
            ILogger<CommentController> logger,
            IHubContext<PostHub> postHub)
        {
            _commentService = commentService;
            _userService = userService;
            _logger = logger;
            _postHub = postHub;
        }

        /// <summary>
        /// Add comment (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateCommentViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid comment data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                request.UserId = userId;

                var result = await _commentService.CreateCommentAsync(request);

                if (result.Success)
                {

                    // Get the created comment with user info for rendering
                    var comment = await _commentService.GetCommentByIdAsync(result.CommentId);
                    //if (comment == null)
                    //{
                    //   //await _postHub.Clients.SendAsync("CommentError", "Failed to retrieve the created comment.");
                    //    return Json(new { success = false, message = "Failed to retrieve the created comment." });
                    //}
                    //else {                         
                    //    // Notify the post hub about the new comment
                    //    await _postHub.Clients.Group($"post-{comment.PostId}").SendAsync("ReceiveComment", comment);
                    //}
                    //// Render the comment partial view
                    //var html = await RenderPartialViewToString("Partials/_CommentItem", comment);


                    return PartialView("~/Views/Shared/Partials/CommentPartial/V1/_CommentItem.cshtml", comment);
                    //return Json(new
                    //{
                    //    success = true,
                    //    commentId = result.CommentId,
                    //    html = html,
                    //    message = "Comment posted successfully!"
                    //});
                }
               

                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return Json(new { success = false, message = "An error occurred while posting your comment." });
            }
        }

        /// <summary>
        /// Handle comment voting (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Vote(int commentId, int voteType)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User is not authenticated." });
                }

                var result = await _commentService.VoteCommentAsync(commentId, userId, voteType);

                if (result.Success)
                {
                    var voteCount = await _commentService.GetCommentVoteCountAsync(commentId);
                    return Json(new
                    {
                        success = true,
                        voteCount = voteCount,
                        userVote = result.UserVote
                    });
                }

                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on comment");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Edit comment (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int commentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment content cannot be empty." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User is not authenticated." });
                }

                // Verify user owns this comment
                var comment = await _commentService.GetCommentByIdAsync(commentId);
                if (comment == null || comment.UserId != userId)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var result = await _commentService.EditCommentAsync(commentId, content, userId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        content = content,
                        editedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        message = "Comment updated successfully!"
                    });
                }

                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment");
                return Json(new { success = false, message = "An error occurred while editing your comment." });
            }
        }

        /// <summary>
        /// Delete comment (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int commentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User is not authenticated." });
                }

                var result = await _commentService.DeleteCommentAsync(commentId, userId);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Comment deleted successfully!"
                    });
                }

                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                return Json(new { success = false, message = "An error occurred while deleting your comment." });
            }
        }

        /// <summary>
        /// Load more comments (AJAX)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoadMore(int postId, int page = 2, string sort = "best")
        {
            try
            {
                var comments = await _commentService.GetPostCommentsAsync(postId, sort, page);

                if (!comments.Any())
                {
                    return Json(new { success = true, hasMore = false });
                }

                // Render comments to HTML
                var html = await RenderPartialViewToString("_CommentsList", comments);

                return Json(new
                {
                    success = true,
                    html = html,
                    hasMore = comments.Count >= 20 // Assuming 20 comments per page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading more comments");
                return Json(new { success = false, message = "Failed to load comments" });
            }
        }

        /// <summary>
        /// Get comment edit form (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEditForm(int commentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var comment = await _commentService.GetCommentByIdAsync(commentId);

                if (comment == null || comment.UserId != userId)
                {
                    return Unauthorized();
                }

                return PartialView("_CommentEditForm", comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment edit form");
                return StatusCode(500);
            }
        }

        #region Helper Methods

        private async Task<string> RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using var writer = new StringWriter();

            var viewEngine = HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
            var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

            if (!viewResult.Success)
            {
                // Try looking in Shared folder
                viewResult = viewEngine.FindView(ControllerContext, $"~/Views/Shared/{viewName}.cshtml", false);
            }

            if (!viewResult.Success || viewResult.View == null)
            {
                throw new InvalidOperationException($"View '{viewName}' not found. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");
            }

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.GetStringBuilder().ToString();
        }

        #endregion
    }
}