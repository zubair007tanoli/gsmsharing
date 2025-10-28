using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace discussionspot9.Services
{
    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailConfiguration _emailConfig;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public EmailService(
            ApplicationDbContext context,
            ILogger<EmailService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            
            // Bind email configuration
            _emailConfig = new EmailConfiguration();
            configuration.GetSection("Email").Bind(_emailConfig);
        }

        #region Core Email Sending

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null)
        {
            if (!_emailConfig.EnableEmails)
            {
                _logger.LogInformation("📧 Emails disabled - would have sent: {Subject} to {Email}", subject, toEmail);
                return false;
            }

            try
            {
                using var client = new SmtpClient(_emailConfig.SmtpHost, _emailConfig.SmtpPort)
                {
                    EnableSsl = _emailConfig.EnableSsl,
                    Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password),
                    Timeout = 30000 // 30 seconds
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(toEmail, toName));

                if (!string.IsNullOrEmpty(plainTextBody))
                {
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain"));
                }

                await client.SendMailAsync(message);
                
                _logger.LogInformation("✅ Email sent successfully to {Email}: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send email to {Email}: {Subject}", toEmail, subject);
                return false;
            }
        }

        public async Task<bool> QueueEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? userId = null, int priority = 5)
        {
            try
            {
                var emailQueue = new EmailQueue
                {
                    ToEmail = toEmail,
                    ToName = toName,
                    Subject = subject,
                    HtmlBody = htmlBody,
                    UserId = userId,
                    Priority = priority,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmailQueues.Add(emailQueue);
                await _context.SaveChangesAsync();

                _logger.LogInformation("📬 Email queued for {Email}: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to queue email for {Email}", toEmail);
                return false;
            }
        }

        #endregion

        #region Notification Emails

        public async Task SendCommentNotificationEmailAsync(Notification notification, string postTitle, string commentContent, string postUrl)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("Cannot send email - user or email not found for UserId: {UserId}", notification.UserId);
                    return;
                }

                // Check user preferences
                if (!await ShouldSendEmailAsync(notification.UserId, "comment"))
                {
                    _logger.LogInformation("User {UserId} has email notifications disabled for comments", notification.UserId);
                    return;
                }

                var template = await LoadEmailTemplateAsync("CommentNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ACTOR_NAME", notification.ActorDisplayName ?? "Someone" },
                    { "ACTOR_AVATAR", notification.ActorAvatarUrl ?? $"{baseUrl}/images/default-avatar.png" },
                    { "POST_TITLE", postTitle },
                    { "COMMENT_CONTENT", TruncateHtml(commentContent, 300) },
                    { "COMMENT_URL", notification.Url ?? postUrl },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"💬 {notification.ActorDisplayName} commented on your post";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 3);

                // Update notification record
                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending comment notification email");
            }
        }

        public async Task SendReplyNotificationEmailAsync(Notification notification, string postTitle, string yourComment, string replyContent, string replyUrl)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                if (!await ShouldSendEmailAsync(notification.UserId, "reply"))
                    return;

                var template = await LoadEmailTemplateAsync("ReplyNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ACTOR_NAME", notification.ActorDisplayName ?? "Someone" },
                    { "ACTOR_AVATAR", notification.ActorAvatarUrl ?? $"{baseUrl}/images/default-avatar.png" },
                    { "POST_TITLE", postTitle },
                    { "YOUR_COMMENT", TruncateHtml(yourComment, 150) },
                    { "REPLY_CONTENT", TruncateHtml(replyContent, 300) },
                    { "REPLY_URL", notification.Url ?? replyUrl },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"💭 {notification.ActorDisplayName} replied to your comment";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 3);

                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reply notification email");
            }
        }

        public async Task SendFollowNotificationEmailAsync(Notification notification, string followerName, string followerProfileUrl, string? followerBio = null)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                if (!await ShouldSendEmailAsync(notification.UserId, "follow"))
                    return;

                var template = await LoadEmailTemplateAsync("FollowNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ACTOR_NAME", followerName },
                    { "ACTOR_AVATAR", notification.ActorAvatarUrl ?? $"{baseUrl}/images/default-avatar.png" },
                    { "ACTOR_PROFILE_URL", followerProfileUrl },
                    { "ACTOR_BIO", followerBio ?? "" },
                    { "FOLLOW_BACK_URL", $"{baseUrl}/api/FollowApi/{notification.ActorUserId}" },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"👤 {followerName} started following you!";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 4);

                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending follow notification email");
            }
        }

        public async Task SendMentionNotificationEmailAsync(Notification notification, string postTitle, string mentionContent, string mentionUrl)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                if (!await ShouldSendEmailAsync(notification.UserId, "mention"))
                    return;

                var template = await LoadEmailTemplateAsync("MentionNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ACTOR_NAME", notification.ActorDisplayName ?? "Someone" },
                    { "ACTOR_AVATAR", notification.ActorAvatarUrl ?? $"{baseUrl}/images/default-avatar.png" },
                    { "ENTITY_TYPE", notification.EntityType ?? "a post" },
                    { "POST_TITLE", postTitle },
                    { "MENTION_CONTENT", TruncateHtml(mentionContent, 250) },
                    { "MENTION_URL", notification.Url ?? mentionUrl },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"@ {notification.ActorDisplayName} mentioned you in a {notification.EntityType}";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 2);

                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending mention notification email");
            }
        }

        public async Task SendDirectMessageEmailAsync(Notification notification, string senderName, string messagePreview, string chatUrl)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                if (!await ShouldSendEmailAsync(notification.UserId, "message"))
                    return;

                var template = await LoadEmailTemplateAsync("DirectMessageNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ACTOR_NAME", senderName },
                    { "ACTOR_AVATAR", notification.ActorAvatarUrl ?? $"{baseUrl}/images/default-avatar.png" },
                    { "MESSAGE_PREVIEW", TruncateHtml(messagePreview, 200) },
                    { "CHAT_URL", chatUrl },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"💬 New message from {senderName}";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 1);

                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending direct message notification email");
            }
        }

        public async Task SendAnnouncementEmailAsync(Notification notification, string title, string message, string? linkUrl = null, string? linkText = null)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                if (!await ShouldSendEmailAsync(notification.UserId, "announcement"))
                    return;

                var template = await LoadEmailTemplateAsync("AnnouncementNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ANNOUNCEMENT_TITLE", title },
                    { "ANNOUNCEMENT_MESSAGE", message },
                    { "ANNOUNCEMENT_LINK", linkUrl ?? "" },
                    { "ANNOUNCEMENT_LINK_TEXT", linkText ?? "Learn More" },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"📢 Announcement: {title}";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 2);

                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending announcement notification email");
            }
        }

        public async Task SendUpvoteNotificationEmailAsync(Notification notification, string entityTitle, string entityPreview, string entityUrl, string entityType)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                if (!await ShouldSendEmailAsync(notification.UserId, "vote"))
                    return;

                var template = await LoadEmailTemplateAsync("UpvoteNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "ACTOR_NAME", notification.ActorDisplayName ?? "Someone" },
                    { "ACTOR_AVATAR", notification.ActorAvatarUrl ?? $"{baseUrl}/images/default-avatar.png" },
                    { "ENTITY_TYPE", entityType },
                    { "ENTITY_TITLE", entityTitle },
                    { "ENTITY_PREVIEW", TruncateHtml(entityPreview, 200) },
                    { "ENTITY_URL", entityUrl },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={notification.UserId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"⬆️ {notification.ActorDisplayName} upvoted your {entityType}";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, notification.UserId, priority: 6);

                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending upvote notification email");
            }
        }

        public async Task SendMilestoneEmailAsync(string userId, string milestoneTitle, string milestoneType, string milestoneValue, string description)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                var userProfile = await _context.UserProfiles.FindAsync(userId);
                var postCount = await _context.Posts.CountAsync(p => p.UserId == userId);
                var commentCount = await _context.Comments.CountAsync(c => c.UserId == userId);

                var template = await LoadEmailTemplateAsync("MilestoneNotification");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "MILESTONE_TITLE", milestoneTitle },
                    { "MILESTONE_TYPE", milestoneType },
                    { "MILESTONE_VALUE", milestoneValue },
                    { "MILESTONE_DESCRIPTION", description },
                    { "USER_POST_COUNT", postCount.ToString() },
                    { "USER_COMMENT_COUNT", commentCount.ToString() },
                    { "USER_KARMA", userProfile?.KarmaPoints.ToString() ?? "0" },
                    { "USER_JOIN_DATE", userProfile?.JoinDate.ToString("MMMM yyyy") ?? "Recently" },
                    { "PROFILE_URL", $"{baseUrl}/u/{userProfile?.DisplayName ?? user.UserName}" },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={userId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = $"🎉 {milestoneTitle}!";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, emailBody, userId, priority: 4);

                _logger.LogInformation("Milestone email queued for {UserId}: {Milestone}", userId, milestoneTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending milestone email");
            }
        }

        #endregion

        #region System Emails

        public async Task SendWelcomeEmailAsync(string userId, string userEmail, string userName)
        {
            try
            {
                var template = await LoadEmailTemplateAsync("WelcomeEmail");
                var baseUrl = GetBaseUrl();

                var placeholders = new Dictionary<string, string>
                {
                    { "USER_NAME", userName },
                    { "BASE_URL", baseUrl },
                    { "UNSUBSCRIBE_URL", $"{baseUrl}/account/unsubscribe?userId={userId}" }
                };

                var emailBody = ReplaceTemplatePlaceholders(template, placeholders);
                var subject = "Welcome to DiscussionSpot! 🎉";

                await QueueEmailAsync(userEmail, userName, subject, emailBody, userId, priority: 1);

                _logger.LogInformation("Welcome email queued for {Email}", userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", userEmail);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #0079d3 0%, #0056b3 100%); padding: 30px; text-align: center; color: white;'>
                            <h1 style='margin: 0; font-size: 32px;'>DISCUSSIONSPOT</h1>
                        </div>
                        <div style='padding: 40px 30px;'>
                            <h2 style='color: #1a1a1b;'>Reset Your Password</h2>
                            <p style='color: #787c7e; line-height: 1.6;'>
                                You requested to reset your password. Click the button below to create a new password.
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' style='background: #0079d3; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: 600; display: inline-block;'>
                                    Reset Password
                                </a>
                            </div>
                            <p style='color: #a8a8a8; font-size: 13px;'>
                                If you didn't request this, please ignore this email. The link expires in 1 hour.
                            </p>
                            <p style='color: #a8a8a8; font-size: 12px; margin-top: 20px;'>
                                Or copy and paste this link: <br>{resetLink}
                            </p>
                        </div>
                        <div style='background: #2c2c2c; color: #a8a8a8; padding: 20px; text-align: center; font-size: 13px;'>
                            <p style='margin: 0;'>© {DateTime.UtcNow.Year} DiscussionSpot. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                await SendEmailAsync(email, "", "Reset Your Password - DiscussionSpot", htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            }
        }

        public async Task SendEmailVerificationAsync(string email, string verificationLink)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background: linear-gradient(135deg, #0079d3 0%, #0056b3 100%); padding: 30px; text-align: center; color: white;'>
                            <h1 style='margin: 0; font-size: 32px;'>DISCUSSIONSPOT</h1>
                        </div>
                        <div style='padding: 40px 30px;'>
                            <h2 style='color: #1a1a1b;'>Verify Your Email Address</h2>
                            <p style='color: #787c7e; line-height: 1.6;'>
                                Thanks for signing up! Please verify your email address to activate your account.
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationLink}' style='background: #46d160; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: 600; display: inline-block;'>
                                    Verify Email Address
                                </a>
                            </div>
                            <p style='color: #a8a8a8; font-size: 13px;'>
                                If you didn't create an account, please ignore this email.
                            </p>
                        </div>
                        <div style='background: #2c2c2c; color: #a8a8a8; padding: 20px; text-align: center; font-size: 13px;'>
                            <p style='margin: 0;'>© {DateTime.UtcNow.Year} DiscussionSpot. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                await SendEmailAsync(email, "", "Verify Your Email - DiscussionSpot", htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to {Email}", email);
            }
        }

        #endregion

        #region Digest Emails

        public async Task SendDailyDigestAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                // Get unread notifications from last 24 hours
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && 
                               !n.IsRead && 
                               n.CreatedAt > DateTime.UtcNow.AddDays(-1))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(20)
                    .ToListAsync();

                if (!notifications.Any())
                {
                    _logger.LogInformation("No notifications for daily digest for user {UserId}", userId);
                    return;
                }

                var digestContent = BuildDigestContent(notifications, "Daily");
                var subject = $"Your Daily Digest - {notifications.Count} New Notifications";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, digestContent, userId, priority: 6);

                _logger.LogInformation("Daily digest queued for {UserId} with {Count} notifications", userId, notifications.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending daily digest for user {UserId}", userId);
            }
        }

        public async Task SendWeeklyDigestAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    return;

                // Get unread notifications from last 7 days
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && 
                               !n.IsRead && 
                               n.CreatedAt > DateTime.UtcNow.AddDays(-7))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                if (!notifications.Any())
                {
                    _logger.LogInformation("No notifications for weekly digest for user {UserId}", userId);
                    return;
                }

                var digestContent = BuildDigestContent(notifications, "Weekly");
                var subject = $"Your Weekly Digest - {notifications.Count} New Notifications";

                await QueueEmailAsync(user.Email, user.UserName ?? "User", subject, digestContent, userId, priority: 7);

                _logger.LogInformation("Weekly digest queued for {UserId} with {Count} notifications", userId, notifications.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending weekly digest for user {UserId}", userId);
            }
        }

        #endregion

        #region Template Management

        public async Task<string> LoadEmailTemplateAsync(string templateName)
        {
            try
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", $"{templateName}.html");
                var layoutPath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "_EmailLayout.html");

                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Email template not found: {Template}", templateName);
                    return string.Empty;
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);
                var layoutContent = File.Exists(layoutPath) ? await File.ReadAllTextAsync(layoutPath) : "{{CONTENT}}";

                // Insert template into layout
                return layoutContent.Replace("{{CONTENT}}", templateContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email template: {Template}", templateName);
                return string.Empty;
            }
        }

        public string ReplaceTemplatePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            var result = template;
            
            foreach (var placeholder in placeholders)
            {
                result = result.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            // Replace any remaining placeholders with empty string
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\{\{[A-Z_]+\}\}", "");
            
            // Handle conditional blocks {{#if X}}...{{/if}}
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\{\{#if\s+\w+\}\}.*?\{\{/if\}\}", "", System.Text.RegularExpressions.RegexOptions.Singleline);

            return result;
        }

        #endregion

        #region Queue Management

        public async Task ProcessEmailQueueAsync()
        {
            try
            {
                var pendingEmails = await _context.EmailQueues
                    .Where(e => e.Status == "pending" && 
                               e.RetryCount < e.MaxRetries &&
                               (e.ScheduledFor == null || e.ScheduledFor <= DateTime.UtcNow))
                    .OrderBy(e => e.Priority)
                    .ThenBy(e => e.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                _logger.LogInformation("Processing {Count} pending emails from queue", pendingEmails.Count);

                foreach (var email in pendingEmails)
                {
                    try
                    {
                        var success = await SendEmailAsync(email.ToEmail, email.ToName ?? "", email.Subject, email.HtmlBody, email.PlainTextBody);

                        if (success)
                        {
                            email.Status = "sent";
                            email.SentAt = DateTime.UtcNow;
                        }
                        else
                        {
                            email.RetryCount++;
                            if (email.RetryCount >= email.MaxRetries)
                            {
                                email.Status = "failed";
                                email.ErrorMessage = "Max retries exceeded";
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email {EmailId}", email.EmailId);
                        email.RetryCount++;
                        email.ErrorMessage = ex.Message;
                        
                        if (email.RetryCount >= email.MaxRetries)
                        {
                            email.Status = "failed";
                        }
                        
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email queue");
            }
        }

        public async Task<int> GetPendingEmailCountAsync()
        {
            return await _context.EmailQueues.CountAsync(e => e.Status == "pending");
        }

        #endregion

        #region Helper Methods

        private async Task<bool> ShouldSendEmailAsync(string userId, string notificationType)
        {
            try
            {
                // Check global email settings
                var settings = await _context.UserNotificationSettings.FindAsync(userId);
                if (settings != null)
                {
                    if (!settings.EmailNotificationsEnabled || settings.UnsubscribeFromAll)
                        return false;

                    // Check quiet hours
                    if (settings.QuietHoursEnabled && IsInQuietHours(settings.QuietHoursStart, settings.QuietHoursEnd))
                        return false;
                }

                // Check specific notification type preference
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference != null && (!preference.EmailEnabled || preference.EmailFrequency == "never"))
                    return false;

                return true;
            }
            catch
            {
                // Default to allowing if there's an error
                return true;
            }
        }

        private bool IsInQuietHours(TimeSpan? start, TimeSpan? end)
        {
            if (!start.HasValue || !end.HasValue)
                return false;

            var now = DateTime.UtcNow.TimeOfDay;

            if (start.Value < end.Value)
            {
                // Normal case: 22:00 - 08:00
                return now >= start.Value && now <= end.Value;
            }
            else
            {
                // Crosses midnight: 22:00 - 02:00
                return now >= start.Value || now <= end.Value;
            }
        }

        private string GetBaseUrl()
        {
            return _configuration["BaseUrl"] ?? "http://localhost:5099";
        }

        private string TruncateHtml(string html, int maxLength)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Strip HTML tags
            var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
            
            if (plainText.Length <= maxLength)
                return System.Net.WebUtility.HtmlEncode(plainText);

            return System.Net.WebUtility.HtmlEncode(plainText.Substring(0, maxLength)) + "...";
        }

        private string BuildDigestContent(List<Notification> notifications, string digestType)
        {
            var sb = new StringBuilder();
            
            foreach (var notification in notifications)
            {
                var icon = notification.Type switch
                {
                    "comment" => "💬",
                    "reply" => "💭",
                    "follow" => "👤",
                    "mention" => "@",
                    "upvote" => "⬆️",
                    _ => "🔔"
                };

                sb.AppendLine($@"
                    <div style='background: #f8f9fa; padding: 15px; margin-bottom: 12px; border-radius: 4px; border-left: 4px solid #0079d3;'>
                        <div style='font-size: 16px; margin-bottom: 5px;'>
                            {icon} <strong>{notification.Title}</strong>
                        </div>
                        <div style='color: #787c7e; font-size: 14px; margin-bottom: 10px;'>
                            {notification.Message}
                        </div>
                        <div style='font-size: 13px; color: #a8a8a8;'>
                            {GetTimeAgo(notification.CreatedAt)}
                        </div>
                        {(notification.Url != null ? $"<a href='{notification.Url}' style='color: #0079d3; text-decoration: none; font-size: 14px; font-weight: 600;'>View →</a>" : "")}
                    </div>");
            }

            var baseUrl = GetBaseUrl();
            return $@"
                <h1 style='color: #0079d3; font-size: 24px;'>Your {digestType} Digest 📬</h1>
                <p style='color: #787c7e;'>You have {notifications.Count} new notifications</p>
                {sb}
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{baseUrl}/notifications' style='background: #0079d3; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: 600; display: inline-block;'>
                        View All Notifications
                    </a>
                </div>";
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
            
            return dateTime.ToString("MMM dd, yyyy");
        }

        #endregion
    }
}

