using discussionspot9.Models.Domain;

namespace discussionspot9.Interfaces
{
    public interface IEmailService
    {
        // Core email sending
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null);
        Task<bool> QueueEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? userId = null, int priority = 5);
        
        // Notification emails
        Task SendCommentNotificationEmailAsync(Notification notification, string postTitle, string commentContent, string postUrl);
        Task SendReplyNotificationEmailAsync(Notification notification, string postTitle, string yourComment, string replyContent, string replyUrl);
        Task SendFollowNotificationEmailAsync(Notification notification, string followerName, string followerProfileUrl, string? followerBio = null);
        Task SendMentionNotificationEmailAsync(Notification notification, string postTitle, string mentionContent, string mentionUrl);
        Task SendDirectMessageEmailAsync(Notification notification, string senderName, string messagePreview, string chatUrl);
        Task SendAnnouncementEmailAsync(Notification notification, string title, string message, string? linkUrl = null, string? linkText = null);
        Task SendUpvoteNotificationEmailAsync(Notification notification, string entityTitle, string entityPreview, string entityUrl, string entityType);
        Task SendMilestoneEmailAsync(string userId, string milestoneTitle, string milestoneType, string milestoneValue, string description);
        
        // System emails
        Task SendWelcomeEmailAsync(string userId, string userEmail, string userName);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task SendEmailVerificationAsync(string email, string verificationLink);
        
        // Digest emails
        Task SendDailyDigestAsync(string userId);
        Task SendWeeklyDigestAsync(string userId);
        
        // Template management
        Task<string> LoadEmailTemplateAsync(string templateName);
        string ReplaceTemplatePlaceholders(string template, Dictionary<string, string> placeholders);
        
        // Queue management
        Task ProcessEmailQueueAsync();
        Task<int> GetPendingEmailCountAsync();
    }
}

