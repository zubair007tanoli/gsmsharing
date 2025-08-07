using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace discussionspot9.Hubs // Ensure this namespace matches your project structure
{
    // The PostHub handles real-time interactions related to posts and comments,
    // including commenting, voting, typing indicators, and notifications.
    public class PostHub : Hub
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PostHub> _logger;
        private readonly IViewRenderService _viewRenderService;

        // Constructor for dependency injection
        public PostHub(IPostService postService, ICommentService commentService,
                      INotificationService notificationService, ILogger<PostHub> logger, IViewRenderService viewRenderService)
        {
            _postService = postService;
            _commentService = commentService;
            _notificationService = notificationService;
            _logger = logger;
            _viewRenderService = viewRenderService;
        }

        [Authorize] // Ensure only authenticated users can cast a vote
        public async Task CastPollVote(int postId, int pollOptionId)
        {
            // Log the vote attempt for debugging purposes
            _logger.LogInformation($"User '{Context.UserIdentifier}' is casting a vote for PostId: {postId}, PollOptionId: {pollOptionId}");

            var userId = GetUserId(); // Assuming GetUserId is a helper method
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID is null or empty. Cannot cast poll vote.");
                // Consider sending a specific error message to the client if needed
                return;
            }

            // Call your service layer to handle the voting logic.
            var pollResult = await _postService.CastPollVoteAsync(postId, pollOptionId, userId);

            if (pollResult.Success)
            {
                // If the vote was successful, broadcast the updated vote counts
                // to all clients in the post's SignalR group.
                // The 'ReceivePollUpdate' is the client-side method name we will define.
                await Clients.Group($"post-{postId}").SendAsync("ReceivePollUpdate", pollResult.UpdatedVoteCounts);
            }
            else
            {
                _logger.LogWarning($"Poll vote failed for PostId: {postId}, PollOptionId: {pollOptionId}. Message: {pollResult.Message}");
            }
        }
        /// <summary>
        /// Allows a client to join a SignalR group specific to a post.
        /// This enables real-time updates for all clients viewing the same post.
        /// </summary>
        /// <param name="postId">The ID of the post to join.</param>
        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}");
            _logger.LogInformation($"User {Context.UserIdentifier} joined post group {postId}");
        }

        /// <summary>
        /// Allows a client to leave a SignalR group specific to a post.
        /// </summary>
        /// <param name="postId">The ID of the post to leave.</param>
        public async Task LeavePostGroup(int postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}");
            _logger.LogInformation($"User {Context.UserIdentifier} left post group {postId}");
        }

        /// <summary>
        /// Handles sending a new comment or a reply to an existing comment.
        /// Requires authentication.
        /// </summary>
        /// <param name="postId">The ID of the post the comment belongs to.</param>
        /// <param name="content">The content of the comment.</param>
        /// <param name="parentCommentId">Optional. The ID of the parent comment if this is a reply.</param>
        [Authorize] // Ensures only authenticated users can send comments
        public async Task SendComment(int postId, string content, int? parentCommentId)
        {
            _logger.LogInformation($"*** DEBUG: SendComment method ENTERED. PostId: {postId}, Content: '{content}', ParentCommentId: {parentCommentId}. ConnectionId: {Context.ConnectionId}");

            var htmlContent = string.Empty;
            var commentId = 0;

            try
            {
                var userId = GetUserId();
                var userName = Context.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    // This scenario should be caught by [Authorize], but provides an additional safeguard.
                    await Clients.Caller.SendAsync("CommentError", "Unauthorized access. Please log in to comment.");
                    return;
                }

                // Create the comment via the CommentService
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

                // Calculate depth for proper indentation in the UI
                int depth = 0;
                if (parentCommentId.HasValue)
                {
                    var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                    depth = parentComment?.TreeLevel + 1 ?? 1; // Increment depth if it's a reply
                }

                // Prepare the view model for rendering the comment HTML
                var commentModel = new CommentTreeViewModel()
                {
                    Comment = comment,
                    Children = new List<CommentTreeViewModel>(),
                    Depth = depth
                };

                // Render the comment partial view to HTML string
                try
                {
                    htmlContent = await _viewRenderService.RenderToStringAsync("Partials/V1/_CommentItem", commentModel);
                }
                catch (Exception renderEx)
                {
                    _logger.LogError(renderEx, "Error rendering comment HTML for PostId {PostId}, CommentId {CommentId}", postId, commentId);
                    await Clients.Caller.SendAsync("CommentError", "Failed to render comment in the UI.");
                    return;
                }

                // Send notifications to post author or parent comment author if different from current user
                var post = await _postService.GetPostByIdAsync(postId);
                if (post != null && post.UserId != userId)
                {
                    await SendNotification(post.UserId, $"{userName} commented on your post", postId, "comment");
                }

                if (parentCommentId.HasValue)
                {
                    var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                    if (parentComment != null && parentComment.UserId != userId && parentComment.UserId != post?.UserId)
                    {
                        // Avoid double-notifying if parent comment author is also post author
                        await SendNotification(parentComment.UserId, $"{userName} replied to your comment", postId, "reply");
                    }
                }

                // Broadcast the new comment HTML to all clients in the post group
                await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", htmlContent, commentId, parentCommentId);
                _logger.LogInformation($"Comment {commentId} for post {postId} sent to group.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending comment for post {PostId}", postId);
                await Clients.Caller.SendAsync("CommentError", "An unexpected error occurred while sending your comment.");
            }
        }

        /// <summary>
        /// Handles editing an existing comment.
        /// Requires authentication.
        /// </summary>
        /// <param name="commentId">The ID of the comment to edit.</param>
        /// <param name="newContent">The new content for the comment.</param>
        [Authorize]
        public async Task EditComment(int commentId, string newContent)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("CommentError", "Unauthorized access. Please log in to edit.");
                    return;
                }

                var result = await _commentService.EditCommentAsync(commentId, newContent, userId);

                if (result.Success)
                {
                    var comment = await _commentService.GetCommentByIdAsync(commentId);
                    if (comment != null)
                    {
                        // Notify clients that the comment has been edited
                        await Clients.Group($"post-{comment.PostId}")
                            .SendAsync("CommentEdited", comment);
                        _logger.LogInformation($"Comment {commentId} edited by {userId} and broadcasted.");
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("CommentError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment {CommentId}", commentId);
                await Clients.Caller.SendAsync("CommentError", "Failed to edit comment.");
            }
        }

        /// <summary>
        /// Handles deleting a comment.
        /// Requires authentication.
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        [Authorize]
        public async Task DeleteComment(int commentId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("CommentError", "User not authenticated to delete comment.");
                    return;
                }

                var comment = await _commentService.GetCommentByIdAsync(commentId);

                if (comment == null)
                {
                    await Clients.Caller.SendAsync("CommentError", "Comment not found.");
                    return;
                }

                var result = await _commentService.DeleteCommentAsync(commentId, userId);

                if (result.Success)
                {
                    // Notify all clients in the post group that the comment was deleted
                    await Clients.Group($"post-{comment.PostId}").SendAsync("CommentDeleted", commentId);
                    _logger.LogInformation($"Comment {commentId} deleted by {userId} and broadcasted.");
                }
                else
                {
                    await Clients.Caller.SendAsync("CommentError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                await Clients.Caller.SendAsync("CommentError", "Failed to delete comment.");
            }
        }

        /// <summary>
        /// Handles voting on a comment (upvote/downvote).
        /// Requires authentication.
        /// </summary>
        /// <param name="commentId">The ID of the comment being voted on.</param>
        /// <param name="voteType">1 for upvote, -1 for downvote, 0 to remove vote.</param>
        [Authorize] // Ensures only authenticated users can vote
        public async Task VoteComment(int commentId, int voteType)
        {
            _logger.LogInformation($"*** DEBUG: VoteComment method ENTERED. CommentId: {commentId}, VoteType: {voteType}. ConnectionId: {Context.ConnectionId}");

            var userId = GetUserId();
            _logger.LogInformation($"VoteComment invoked for commentId: {commentId}, voteType: {voteType}, UserId: {userId ?? "null"}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning($"Unauthorized attempt to vote on comment {commentId}. User ID is null or empty.");
                await Clients.Caller.SendAsync("VoteError", "Unauthorized access. Please log in to vote.");
                return;
            }

            try
            {
                var result = await _commentService.VoteCommentAsync(commentId, userId, voteType);

                if (result.Success)
                {
                    var comment = await _commentService.GetCommentByIdAsync(commentId);
                    if (comment != null)
                    {

          
                        // Broadcast updated vote counts and user's vote status to all clients in the post group
                        await Clients.Group($"post-{comment.PostId}")
                            .SendAsync("CommentVoteUpdated", commentId,
                                comment.UpvoteCount, comment.DownvoteCount, comment.Score); // Pass UserVote here
                        _logger.LogInformation($"Successfully updated comment vote for comment {commentId}. New score: {comment.Score}, Up: {comment.UpvoteCount}, Down: {comment.DownvoteCount}, UserVote: {result.UserVote}.");
                    }
                    else
                    {
                        _logger.LogError($"Comment with ID {commentId} not found after successful vote in service.");
                        await Clients.Caller.SendAsync("VoteError", "Comment not found after voting. Please refresh.");
                    }
                }
                else
                {
                    _logger.LogError($"Failed to vote on comment {commentId}. Error: {result.ErrorMessage}");
                    await Clients.Caller.SendAsync("VoteError", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception while voting on comment {commentId}");
                await Clients.Caller.SendAsync("VoteError", "An unexpected error occurred while voting on comment.");
            }
        }

        /// <summary>
        /// Handles voting on a post.
        /// Requires authentication.
        /// </summary>
        /// <param name="postId">The ID of the post being voted on.</param>
        /// <param name="voteType">1 for upvote, -1 for downvote, 0 to remove vote.</param>
        [Authorize]
        public async Task VotePost(int postId, int voteType)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("VoteError", "User not authenticated to vote on post.");
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
                        _logger.LogInformation($"Post {postId} vote updated. Up: {updatedPost.UpvoteCount}, Down: {updatedPost.DownvoteCount}, UserVote: {result.UserVote}.");
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

        /// <summary>
        /// Notifies other users in the post group that the current user has started typing a comment.
        /// Requires authentication.
        /// </summary>
        /// <param name="postId">The ID of the post where typing is occurring.</param>
        [Authorize]
        public async Task StartTyping(int postId)
        {
            var userName = Context.User?.Identity?.Name; // Get display name if available
            if (!string.IsNullOrEmpty(userName))
            {
                // Notify others in the group (excluding the sender)
                await Clients.OthersInGroup($"post-{postId}").SendAsync("UserStartedTyping", userName);
                _logger.LogInformation($"User {userName} started typing in post {postId}.");
            }
        }

        /// <summary>
        /// Notifies other users in the post group that the current user has stopped typing.
        /// Requires authentication.
        /// </summary>
        /// <param name="postId">The ID of the post where typing occurred.</param>
        [Authorize]
        public async Task StopTyping(int postId)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                // Notify others in the group (excluding the sender)
                await Clients.OthersInGroup($"post-{postId}").SendAsync("UserStoppedTyping", userName);
                _logger.LogInformation($"User {userName} stopped typing in post {postId}.");
            }
        }

        /// <summary>
        /// Helper method to send a notification to a specific user.
        /// Integrates with the NotificationService to persist the notification.
        /// </summary>
        /// <param name="userId">The ID of the user to notify.</param>
        /// <param name="message">The notification message.</param>
        /// <param name="postId">The related post ID.</param>
        /// <param name="type">Type of notification (e.g., "comment", "reply", "award").</param>
        private async Task SendNotification(string userId, string message, int postId, string type)
        {
            try
            {
                // Persist notification to database
                await _notificationService.CreateNotificationAsync(userId, message, postId, type);

                // Send real-time notification to the target user's group
                await Clients.User(userId).SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    PostId = postId,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                });
                _logger.LogInformation($"Notification sent to user {userId}: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's ID from the SignalR context.
        /// </summary>
        /// <returns>The user ID as a string, or null if not authenticated.</returns>
        private string? GetUserId() =>
            Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>
        /// Called when a new client connects to the Hub.
        /// Adds authenticated users to a user-specific group for direct notifications.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                    _logger.LogInformation($"User {userId} connected to SignalR. ConnectionId: {Context.ConnectionId}");
                }
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the Hub.
        /// Removes authenticated users from their user-specific group.
        /// </summary>
        /// <param name="exception">The exception that caused the disconnect, if any.</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
                    _logger.LogInformation($"User {userId} disconnected from SignalR. ConnectionId: {Context.ConnectionId}");
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
