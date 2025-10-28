
namespace discussionspot9.Interfaces
{
    public interface INotificationService
    {
        // Award notifications
        Task CreateAwardNotificationAsync(int postId, int awardId, string fromUserId);
        
        // Generic notification
        Task CreateNotificationAsync(string userId, string title, string message, string entityType, string entityId, string type);
        
        // Comment notifications
        Task NotifyPostCommentAsync(int postId, int commentId, string commenterUserId, string commenterName);
        Task NotifyCommentReplyAsync(int parentCommentId, int replyCommentId, string replierUserId, string replierName);
        
        // Vote notifications
        Task NotifyPostVoteAsync(int postId, string voterUserId, string voterName, int voteType);
        Task NotifyCommentVoteAsync(int commentId, string voterUserId, string voterName, int voteType);
        
        // Mention notifications
        Task NotifyMentionsAsync(string content, string actorUserId, string actorName, string entityType, int entityId, string entityUrl);
        
        // Utility methods
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string? userId);
    }
}
