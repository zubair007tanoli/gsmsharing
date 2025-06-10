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

        public PostHub(IPostService postService, ICommentService commentService,
                      INotificationService notificationService, ILogger<PostHub> logger)
        {
            _postService = postService;
            _commentService = commentService;
            _notificationService = notificationService;
            _logger = logger;
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
            try
            {
                var userId = GetUserId();
                var userName = Context.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException();

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

                var comment = await _commentService.GetCommentByIdAsync(commentResult.CommentId);
                await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", comment);

                // Send notifications only if not commenting on own content
                var post = await _postService.GetPostByIdAsync(postId);
                if (post != null && post.UserId != userId)
                {
                    await SendNotification(
                        post.UserId,
                        $"{userName} commented on your post",
                        postId,
                        "comment"
                    );
                }

                if (parentCommentId.HasValue)
                {
                    var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                    if (parentComment != null && parentComment.UserId != userId)
                    {
                        await SendNotification(
                            parentComment.UserId,
                            $"{userName} replied to your comment",
                            postId,
                            "reply"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending comment");
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
                            comment.UpvoteCount, comment.DownvoteCount);
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
        public async Task UpdateVoteCount(int postId, int newCount)
        {
            await Clients.Group($"post-{postId}").SendAsync("UpdateVoteCount", newCount);
        }

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
                    userId, message, postId, type);

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