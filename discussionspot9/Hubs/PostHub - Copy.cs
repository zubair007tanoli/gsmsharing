using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace discussionspot9.Hubs
{
    public class PostHub : Hub
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PostHub> _logger;
        private readonly IViewRenderService _viewRenderService;

        public PostHub(IPostService postService, ICommentService commentService,
                      INotificationService notificationService, ILogger<PostHub> logger, IViewRenderService viewRenderService)
        {
            _postService = postService;
            _commentService = commentService;
            _notificationService = notificationService;
            _logger = logger;
            _viewRenderService = viewRenderService;
        }

        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}");
            _logger.LogInformation($"User {Context.UserIdentifier} joined post group {postId}");
        }

        public async Task LeavePostGroup(int postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}");
            _logger.LogInformation($"User {Context.UserIdentifier} left post group {postId}");
        }

        [Authorize]
        public async Task SendComment(int postId, string content, int? parentCommentId)
        {
            var htmlContent = string.Empty;
            var commentId = 0;

            try
            {
                var userId = GetUserId();
                var userName = Context.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("CommentError", "Unauthorized access");
                    return;
                }

                var commentResult = await _commentService.CreateCommentAsync(new CreateCommentViewModel
                {
                    PostId = postId,
                    Content = content,
                    ParentCommentId = parentCommentId,
                    UserId = userId
                });

                if (!commentResult.Success)
                {
                    await Clients.Caller.SendAsync("CommentError", commentResult.ErrorMessage);
                    return;
                }

                commentId = commentResult.CommentId;
                var comment = await _commentService.GetCommentByIdAsync(commentId);

                // Calculate depth for proper indentation
                int depth = 0;
                if (parentCommentId.HasValue)
                {
                    var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                    depth = parentComment?.TreeLevel + 1 ?? 1;
                }

                var commentModel = new CommentTreeViewModel()
                {
                    Comment = comment,
                    Children = new List<CommentTreeViewModel>(),
                    Depth = depth
                };

                try
                {
                    htmlContent = await _viewRenderService.RenderToStringAsync("Partials/V1/_CommentItem", commentModel);
                }
                catch (Exception renderEx)
                {
                    _logger.LogError(renderEx, "Error rendering comment HTML");
                    await Clients.Caller.SendAsync("CommentError", "Failed to render comment");
                    return;
                }

                // Send notifications (existing code)
                var post = await _postService.GetPostByIdAsync(postId);
                if (post != null && post.UserId != userId)
                {
                    await SendNotification(post.UserId, $"{userName} commented on your post", postId, "comment");
                }

                if (parentCommentId.HasValue)
                {
                    var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                    if (parentComment != null && parentComment.UserId != userId)
                    {
                        await SendNotification(parentComment.UserId, $"{userName} replied to your comment", postId, "reply");
                    }
                }

                await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", htmlContent, commentId, parentCommentId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending comment for post {PostId}", postId);
                await Clients.Caller.SendAsync("CommentError", "Failed to send comment");
            }
        }

        [Authorize]
        public async Task EditComment(int commentId, string newContent)
        {
            try
            {
                var userId = GetUserId();
                var result = await _commentService.EditCommentAsync(commentId, newContent, userId);

                if (result.Success)
                {
                    var comment = await _commentService.GetCommentByIdAsync(commentId);
                    await Clients.Group($"post-{comment.PostId}")
                        .SendAsync("CommentEdited", comment);
                }
                else
                {
                    await Clients.Caller.SendAsync("CommentError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment");
                await Clients.Caller.SendAsync("CommentError", "Failed to edit comment");
            }
        }

        [Authorize]
        public async Task DeleteComment(int commentId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("CommentError", "User not authenticated");
                    return;
                }

                var comment = await _commentService.GetCommentByIdAsync(commentId);

                if (comment == null)
                {
                    await Clients.Caller.SendAsync("CommentError", "Comment not found");
                    return;
                }

                var result = await _commentService.DeleteCommentAsync(commentId, userId);

                if (result.Success)
                {
                    await Clients.Group($"post-{comment.PostId}").SendAsync("CommentDeleted", commentId);
                }
                else
                {
                    await Clients.Caller.SendAsync("CommentError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                await Clients.Caller.SendAsync("CommentError", "Failed to delete comment");
            }
        }

        [Authorize]
        public async Task VoteComment(int commentId, int voteType)
        {
            try
            {
                var userId = GetUserId();
                var result = await _commentService.VoteCommentAsync(commentId, userId, voteType);

                if (result.Success)
                {
                    var comment = await _commentService.GetCommentByIdAsync(commentId);
                    await Clients.Group($"post-{comment.PostId}")
                        .SendAsync("CommentVoteUpdated", commentId,
                            comment.UpvoteCount, comment.DownvoteCount, result.UserVote); // Pass UserVote here
                }
                else
                {
                    await Clients.Caller.SendAsync("VoteError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on comment");
                await Clients.Caller.SendAsync("VoteError", "Failed to vote on comment");
            }
        }

        // Modified: To send more detailed vote information
        [Authorize]
        public async Task VotePost(int postId, int voteType)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("VoteError", "User not authenticated.");
                    return;
                }

                var result = await _postService.VotePostAsync(postId, userId, voteType);

                if (result.Success)
                {

                    var updatedPost = await _postService.GetPostByIdAsync(postId);
                    if (updatedPost != null)
                    {
                        // Send all necessary info: postId, new upvote, new downvote, and current user's vote status
                        await Clients.Group($"post-{postId}").SendAsync("UpdatePostVotesUI",
                            postId,
                            updatedPost.UpvoteCount,
                            updatedPost.DownvoteCount,
                            result.UserVote // This should be 1, -1, or null (for removed vote)
                        );
                       
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("VoteError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on post {PostId}", postId);
                await Clients.Caller.SendAsync("VoteError", "Failed to vote on post.");
            }
        }

        // Removed: UpdateVoteCount as it's now replaced by UpdatePostVotesUI
        // public async Task UpdateVoteCount(int postId, int newCount)
        // {
        //     await Clients.Group($"post-{postId}").SendAsync("UpdateVoteCount", newCount);
        // }

        [Authorize]
        public async Task StartTyping(int postId)
        {
            var userName = Context.User?.Identity?.Name;
            await Clients.OthersInGroup($"post-{postId}").SendAsync("UserStartedTyping", userName);
        }

        [Authorize]
        public async Task StopTyping(int postId)
        {
            var userName = Context.User?.Identity?.Name;
            await Clients.OthersInGroup($"post-{postId}").SendAsync("UserStoppedTyping", userName);
        }
        private async Task SendNotification(string userId, string message, int postId, string type)
        {
            try
            {
                await _notificationService.CreateNotificationAsync(
                    userId, 
                    "New Activity",  // title
                    message, 
                    "post",          // entityType
                    postId.ToString(), // entityId
                    type);

                await Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    PostId = postId,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }

        private string? GetUserId() =>
            Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} connected to SignalR");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} disconnected from SignalR");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
