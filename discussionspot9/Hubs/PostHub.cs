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
            var userId = GetUserId();
            _logger.LogInformation($"Attempting to cast poll vote. User: '{userId}', PostId: {postId}, OptionId: {pollOptionId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized vote attempt: User ID is null or empty.");
                await Clients.Caller.SendAsync("ReceivePollVoteError", "You must be logged in to vote.");
                return;
            }

            try
            {
                // **FIX:** The premature check for whether a user has already voted has been removed.
                // The service layer (`PostService`) is designed to handle vote changes (i.e., removing
                // the old vote and adding the new one). This hub was incorrectly preventing that logic
                // from ever being reached. By removing the block, we allow the service to correctly
                // process new votes and vote changes.

                // Call the service layer to handle the voting logic.
                var pollResult = await _postService.CastPollVoteAsync(postId, pollOptionId, userId);

                if (pollResult.Success)
                {
                    _logger.LogInformation($"Successfully cast vote for PostId: {postId} by User: '{userId}'.");

                    // **FIX:** To ensure correct UI updates for all clients, we now generate two separate payloads.
                    // 1. A personalized payload for the user who cast the vote, which will correctly
                    //    show their selected option and "You have voted" status.
                    var pollDataForCaller = await _postService.GetPollDetailsAsync(postId, userId);

                    // 2. A generic payload for all other users in the group. This payload contains
                    //    the updated vote counts and percentages but does not include any user-specific
                    //    data, so it won't incorrectly change their view of the poll.
                    var pollDataForOthers = await _postService.GetPollDetailsAsync(postId, null);

                    // Send the personalized update directly to the user who voted.
                    await Clients.Caller.SendAsync("ReceivePollUpdate", pollDataForCaller);

                    // Broadcast the generic update to everyone else viewing the post.
                    await Clients.OthersInGroup($"post-{postId}").SendAsync("ReceivePollUpdate", pollDataForOthers);

                    // Send a success notification toast to the caller.
                    await Clients.Caller.SendAsync("ReceivePollVoteSuccess", "Your vote was recorded successfully!");
                }
                else
                {
                    // If the service call failed, send an error back to the caller.
                    _logger.LogError($"Poll vote failed for PostId: {postId}. Message: {pollResult.Message}");
                    await Clients.Caller.SendAsync("ReceivePollVoteError", pollResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while casting poll vote for PostId: {postId}");
                await Clients.Caller.SendAsync("ReceivePollVoteError", "An unexpected error occurred. Please try again.");
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
            _logger.LogInformation($"Comment submission received for PostId: {postId}.");

            var htmlContent = string.Empty;
            var commentId = 0;

            try
            {
                var userId = GetUserId();
                var userName = Context.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("CommentError", "Unauthorized access. Please log in to comment.");
                    return;
                }

                // --- PERFORMANCE OPTIMIZATION SUGGESTION ---
                // Consider modifying your CreateCommentAsync service method to return the full comment object.
                // This would avoid the second database call (`GetCommentByIdAsync`) and speed up the response.
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

                if (comment == null)
                {
                    _logger.LogError("Comment could not be found with ID {CommentId} after creation.", commentId);
                    await Clients.Caller.SendAsync("CommentError", "Could not retrieve comment after saving.");
                    return;
                }

                // Process link previews asynchronously (with timeout protection)
                try
                {
                    var linkPreviewTask = _commentService.ProcessLinkPreviewsAsync(commentId, content);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3)); // 3-second timeout for link previews
                    
                    var completedTask = await Task.WhenAny(linkPreviewTask, timeoutTask);
                    
                    if (completedTask == linkPreviewTask)
                    {
                        // Link previews completed within timeout
                        comment.LinkPreviews = await linkPreviewTask;
                        _logger.LogInformation($"Link previews processed successfully for comment {commentId}");
                    }
                    else
                    {
                        // Timeout occurred, continue without link previews (they'll be saved in background)
                        _logger.LogWarning($"Link preview processing timed out for comment {commentId}, proceeding without previews");
                        comment.LinkPreviews = new List<LinkPreviewViewModel>();
                        
                        // Let the task continue in background (fire and forget)
                        _ = linkPreviewTask.ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                _logger.LogInformation($"Background link preview processing completed for comment {commentId}");
                            }
                            else if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, $"Background link preview processing failed for comment {commentId}");
                            }
                        });
                    }
                }
                catch (Exception linkEx)
                {
                    _logger.LogError(linkEx, $"Error processing link previews for comment {commentId}");
                    comment.LinkPreviews = new List<LinkPreviewViewModel>();
                }

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

                // Render the partial view to an HTML string on the server.
                htmlContent = await _viewRenderService.RenderToStringAsync("Partials/V1/_CommentItem", commentModel);

                // Send notifications
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
                        await SendNotification(parentComment.UserId, $"{userName} replied to your comment", postId, "reply");
                    }
                }

                // Broadcast the final rendered HTML to all clients in the group.
                await Clients.Group($"post-{postId}").SendAsync("ReceiveComment", htmlContent, commentId, parentCommentId);
                _logger.LogInformation($"Successfully broadcasted comment {commentId} for post {postId} to group.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in SendComment for post {PostId}", postId);
                // Inform the caller that their comment failed.
                // The client-side JS should handle removing the optimistic comment.
                await Clients.Caller.SendAsync("CommentError", "An unexpected error occurred. Your comment was not posted.");
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
                        _logger.LogInformation($"📊 Comment {commentId} vote updated. Up: {comment.UpvoteCount}, Down: {comment.DownvoteCount}, Score: {comment.Score}, UserVote: {result.UserVote}");
                        _logger.LogInformation($"📤 Sending to Caller - CommentId: {commentId}, Up: {comment.UpvoteCount}, Down: {comment.DownvoteCount}, Score: {comment.Score}, UserVote: {result.UserVote}");

                        // Send update to the caller (the user who voted) with their vote status
                        await Clients.Caller.SendAsync("CommentVoteUpdated", commentId,
                            comment.UpvoteCount, comment.DownvoteCount, comment.Score, result.UserVote);
                        
                        _logger.LogInformation($"📤 Sending to OthersInGroup - CommentId: {commentId}, Up: {comment.UpvoteCount}, Down: {comment.DownvoteCount}, Score: {comment.Score}");
                        
                        // Send update to others in the post group (without user vote status)
                        await Clients.OthersInGroup($"post-{comment.PostId}").SendAsync("CommentVoteUpdated", commentId,
                            comment.UpvoteCount, comment.DownvoteCount, comment.Score, null);
                        
                        _logger.LogInformation($"✅ Comment vote broadcasted successfully for comment {commentId}");
                    }
                    else
                    {
                        _logger.LogError($"❌ Comment with ID {commentId} not found after successful vote in service.");
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
                    // Get fresh post data from database (not cached)
                    var updatedPost = await _postService.GetPostByIdAsync(postId);
                    
                    if (updatedPost != null)
                    {
                        _logger.LogInformation($"📊 Post {postId} vote updated. Up: {updatedPost.UpvoteCount}, Down: {updatedPost.DownvoteCount}, UserVote: {result.UserVote}, Score: {updatedPost.Score}");
                        
                        // CRITICAL: Use the vote counts that were just saved in VotePostAsync
                        // The updatedPost might have stale data due to EF Core tracking
                        // So we trust the result and send the data
                        
                        _logger.LogInformation($"📤 Sending to Caller - UpvoteCount: {updatedPost.UpvoteCount}, DownvoteCount: {updatedPost.DownvoteCount}, UserVote: {result.UserVote}");
                        
                        // Send update to the caller (the user who voted) with their vote status
                        await Clients.Caller.SendAsync("UpdatePostVotesUI",
                            postId,
                            updatedPost.UpvoteCount,
                            updatedPost.DownvoteCount,
                            result.UserVote
                        );
                        
                        _logger.LogInformation($"📤 Sending to OthersInGroup - UpvoteCount: {updatedPost.UpvoteCount}, DownvoteCount: {updatedPost.DownvoteCount}");
                        
                        // Send update to others in the group (without user-specific vote status)
                        await Clients.OthersInGroup($"post-{postId}").SendAsync("UpdatePostVotesUI",
                            postId,
                            updatedPost.UpvoteCount,
                            updatedPost.DownvoteCount,
                            null // Others don't get the voter's vote status
                        );
                    }
                    else
                    {
                        _logger.LogError($"❌ Could not retrieve post {postId} after voting");
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
                await _notificationService.CreateNotificationAsync(
                    userId, 
                    "New Activity",  // title
                    message, 
                    "post",          // entityType
                    postId.ToString(), // entityId
                    type);

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
