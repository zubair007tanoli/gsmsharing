using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> notificationHub,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _notificationHub = notificationHub;
            _logger = logger;
        }

        #region Generic Notification Methods

        public async Task CreateNotificationAsync(
            string userId,
            string title,
            string message,
            string entityType,
            string entityId,
            string type)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    EntityType = entityType,
                    EntityId = entityId,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification created for user {userId}: {title}");

                // Send real-time notification via SignalR
                await SendRealtimeNotification(userId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating notification for user {userId}");
            }
        }

        #endregion

        #region Comment Notifications

        /// <summary>
        /// Notify post author when someone comments on their post
        /// </summary>
        public async Task NotifyPostCommentAsync(int postId, int commentId, string commenterUserId, string commenterName)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null) return;

                // Don't notify if user is commenting on their own post
                if (post.UserId == commenterUserId) return;

                var notification = new Notification
                {
                    UserId = post.UserId, // Post author
                    Title = "New Comment",
                    Message = $"{commenterName} commented on your post \"{TruncateText(post.Title, 50)}\"",
                    EntityType = "comment",
                    EntityId = commentId.ToString(),
                    Type = "comment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Post comment notification created: PostId={postId}, Commenter={commenterName}");

                // Send real-time notification
                await SendRealtimeNotification(post.UserId, notification, 
                    url: $"/r/{post.Community.Slug}/posts/{post.Slug}#{commentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating post comment notification for PostId={postId}");
            }
        }

        /// <summary>
        /// Notify comment author when someone replies to their comment
        /// </summary>
        public async Task NotifyCommentReplyAsync(int parentCommentId, int replyCommentId, string replierUserId, string replierName)
        {
            try
            {
                var parentComment = await _context.Comments
                    .Include(c => c.Post)
                        .ThenInclude(p => p.Community)
                    .FirstOrDefaultAsync(c => c.CommentId == parentCommentId);

                if (parentComment == null) return;

                // Don't notify if user is replying to their own comment
                if (parentComment.UserId == replierUserId) return;

                var notification = new Notification
                {
                    UserId = parentComment.UserId, // Parent comment author
                    Title = "New Reply",
                    Message = $"{replierName} replied to your comment",
                    EntityType = "comment",
                    EntityId = replyCommentId.ToString(),
                    Type = "reply",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Comment reply notification created: ParentCommentId={parentCommentId}, Replier={replierName}");

                // Send real-time notification
                await SendRealtimeNotification(parentComment.UserId, notification,
                    url: $"/r/{parentComment.Post.Community.Slug}/posts/{parentComment.Post.Slug}#{replyCommentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating comment reply notification for ParentCommentId={parentCommentId}");
            }
        }

        #endregion

        #region Vote Notifications

        /// <summary>
        /// Notify post author when someone upvotes their post (only for upvotes, not downvotes)
        /// </summary>
        public async Task NotifyPostVoteAsync(int postId, string voterUserId, string voterName, int voteType)
        {
            // Only notify for upvotes
            if (voteType != 1) return;

            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null) return;

                // Don't notify if user is voting on their own post
                if (post.UserId == voterUserId) return;

                // Check if we've already notified about this user's vote recently (prevent spam)
                var recentNotification = await _context.Notifications
                    .Where(n => n.UserId == post.UserId &&
                                n.EntityType == "post" &&
                                n.EntityId == postId.ToString() &&
                                n.Type == "upvote" &&
                                n.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
                    .AnyAsync();

                if (recentNotification) return; // Already notified recently

                var notification = new Notification
                {
                    UserId = post.UserId, // Post author
                    Title = "Post Upvoted",
                    Message = $"{voterName} upvoted your post \"{TruncateText(post.Title, 50)}\"",
                    EntityType = "post",
                    EntityId = postId.ToString(),
                    Type = "upvote",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Post vote notification created: PostId={postId}, Voter={voterName}");

                // Send real-time notification
                await SendRealtimeNotification(post.UserId, notification,
                    url: $"/r/{post.Community.Slug}/posts/{post.Slug}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating post vote notification for PostId={postId}");
            }
        }

        /// <summary>
        /// Notify comment author when someone upvotes their comment (only for upvotes)
        /// </summary>
        public async Task NotifyCommentVoteAsync(int commentId, string voterUserId, string voterName, int voteType)
        {
            // Only notify for upvotes
            if (voteType != 1) return;

            try
            {
                var comment = await _context.Comments
                    .Include(c => c.Post)
                        .ThenInclude(p => p.Community)
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                if (comment == null) return;

                // Don't notify if user is voting on their own comment
                if (comment.UserId == voterUserId) return;

                // Check if we've already notified about this user's vote recently (prevent spam)
                var recentNotification = await _context.Notifications
                    .Where(n => n.UserId == comment.UserId &&
                                n.EntityType == "comment" &&
                                n.EntityId == commentId.ToString() &&
                                n.Type == "upvote" &&
                                n.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
                    .AnyAsync();

                if (recentNotification) return; // Already notified recently

                var notification = new Notification
                {
                    UserId = comment.UserId, // Comment author
                    Title = "Comment Upvoted",
                    Message = $"{voterName} upvoted your comment",
                    EntityType = "comment",
                    EntityId = commentId.ToString(),
                    Type = "upvote",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Comment vote notification created: CommentId={commentId}, Voter={voterName}");

                // Send real-time notification
                await SendRealtimeNotification(comment.UserId, notification,
                    url: $"/r/{comment.Post.Community.Slug}/posts/{comment.Post.Slug}#{commentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating comment vote notification for CommentId={commentId}");
            }
        }

        #endregion

        #region Award Notifications

        public async Task CreateAwardNotificationAsync(int postId, int awardId, string fromUserId)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                var award = await _context.Awards.FindAsync(awardId);
                var giver = await _context.UserProfiles.FindAsync(fromUserId);

                if (post == null || award == null || giver == null) return;

                // Don't notify if user is giving award to their own post
                if (post.UserId == fromUserId) return;

                var notification = new Notification
                {
                    UserId = post.UserId,
                    Title = "Award Received!",
                    Message = $"{giver.DisplayName} gave your post \"{TruncateText(post.Title, 50)}\" the {award.Name} award!",
                    EntityType = "post",
                    EntityId = postId.ToString(),
                    Type = "award",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Award notification created: PostId={postId}, Award={award.Name}");

                // Send real-time notification
                await SendRealtimeNotification(post.UserId, notification,
                    url: $"/r/{post.Community.Slug}/posts/{post.Slug}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating award notification for PostId={postId}");
            }
        }

        #endregion

        #region Utility Methods

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId, string? userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Notification {notificationId} marked as read for user {userId}");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Send real-time notification via SignalR
        /// </summary>
        private async Task SendRealtimeNotification(string userId, Notification notification, string? url = null)
        {
            try
            {
                await _notificationHub.Clients.Group($"notifications-{userId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        notificationId = notification.NotificationId,
                        type = notification.Type,
                        title = notification.Title,
                        message = notification.Message,
                        url = url,
                        createdAt = notification.CreatedAt,
                        isRead = notification.IsRead
                    });

                _logger.LogInformation($"Real-time notification sent to user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending real-time notification to user {userId}");
            }
        }

        /// <summary>
        /// Truncate text to specified length and add ellipsis
        /// </summary>
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }

        #endregion
    }
}
