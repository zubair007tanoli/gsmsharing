# 📧 Complete Email Notification System

## ✅ Implementation Status: **COMPLETE**

This document provides a comprehensive overview of the fully implemented email notification system for DiscussionSpot.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Notification Types](#notification-types)
3. [Database Schema](#database-schema)
4. [Email Templates](#email-templates)
5. [Configuration](#configuration)
6. [User Preferences](#user-preferences)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

The email notification system sends automated emails to users for various events:
- **Real-time notifications** for immediate actions (comments, messages)
- **Queued delivery** for reliable sending
- **User preferences** to control what emails they receive
- **Smart offline detection** - only sends emails if user is offline
- **Beautiful HTML templates** with responsive design
- **Dark mode compatible** for Gmail/Outlook

---

## 📬 Notification Types

### 1. **Comment Notifications** 💬
**Trigger:** Someone comments on your post  
**Priority:** High (1)  
**Template:** `CommentNotification.html`  
**Features:**
- Shows commenter's avatar and name
- Preview of the comment content
- Direct link to the post
- Encourages engagement

**Usage:**
```csharp
await _notificationService.CreateNotificationAsync(
    userId: postOwnerId,
    title: "New Comment",
    message: $"{commenterName} commented on your post",
    entityType: "post",
    entityId: postId,
    type: "comment");
```

---

### 2. **Reply Notifications** 💭
**Trigger:** Someone replies to your comment  
**Priority:** High (1)  
**Template:** `ReplyNotification.html`  
**Features:**
- Shows your original comment
- Displays the reply
- Direct link to the conversation
- Maintains thread context

**Usage:**
```csharp
await _notificationService.CreateNotificationAsync(
    userId: originalCommenterId,
    title: "New Reply",
    message: $"{replierName} replied to your comment",
    entityType: "comment",
    entityId: commentId,
    type: "reply");
```

---

### 3. **Follow Notifications** 👥
**Trigger:** Someone follows you  
**Priority:** Medium (3)  
**Template:** `FollowNotification.html`  
**Features:**
- Shows follower's profile info
- Displays bio if available
- Link to follower's profile
- Follow back suggestion

**Usage:**
```csharp
await _notificationService.CreateNotificationAsync(
    userId: followedUserId,
    title: "New Follower",
    message: $"{followerName} started following you",
    entityType: "user",
    entityId: followerId,
    type: "follow");
```

---

### 4. **Mention Notifications** 📢
**Trigger:** Someone @mentions you in a post or comment  
**Priority:** High (2)  
**Template:** `MentionNotification.html`  
**Features:**
- Shows who mentioned you
- Preview of the mention context
- Direct link to the mention
- Highlights your username

**Usage:**
```csharp
await _notificationService.NotifyMentionsAsync(
    content: postContent,
    actorUserId: authorId,
    actorName: authorName,
    entityType: "post",
    entityId: postId,
    entityUrl: postUrl);
```

---

### 5. **Direct Message Notifications** 💌
**Trigger:** Someone sends you a private message  
**Priority:** Highest (1)  
**Template:** `DirectMessageNotification.html`  
**Features:**
- Shows sender's avatar
- Message preview (first 200 chars)
- Direct link to chat
- Emphasizes quick reply

**Usage:**
```csharp
await _emailService.SendDirectMessageEmailAsync(
    notification,
    senderName: "John Doe",
    messagePreview: messageContent,
    chatUrl: "/chat/user123");
```

---

### 6. **Announcement Notifications** 📣
**Trigger:** Admin posts a site-wide announcement  
**Priority:** Important (2)  
**Template:** `AnnouncementNotification.html`  
**Features:**
- Eye-catching gradient banner
- Announcement title and message
- Optional CTA link
- Professional branding

**Usage:**
```csharp
await _emailService.SendAnnouncementEmailAsync(
    notification,
    title: "New Features Released!",
    message: "We've added dark mode and mobile app...",
    linkUrl: "/updates",
    linkText: "View Details");
```

---

### 7. **Upvote Notifications** ⬆️
**Trigger:** Someone upvotes your post or comment  
**Priority:** Low (6)  
**Template:** `UpvoteNotification.html`  
**Features:**
- Animated upvote icon
- Shows what was upvoted
- Encourages content creation
- Links to the content

**Usage:**
```csharp
await _emailService.SendUpvoteNotificationEmailAsync(
    notification,
    entityTitle: "My Amazing Post",
    entityPreview: postContent,
    entityUrl: postUrl,
    entityType: "post");
```

---

### 8. **Milestone Notifications** 🎉
**Trigger:** User reaches a milestone (1k karma, 100 posts, etc.)  
**Priority:** Medium (4)  
**Template:** `MilestoneNotification.html`  
**Features:**
- Celebratory design
- Shows user stats
- Milestone details
- Profile link

**Usage:**
```csharp
await _emailService.SendMilestoneEmailAsync(
    userId,
    milestoneTitle: "1,000 Karma Points!",
    milestoneType: "Karma",
    milestoneValue: "1,000",
    description: "You've earned respect from the community");
```

---

### 9. **Welcome Email** 🎊
**Trigger:** New user registration  
**Priority:** High (1)  
**Template:** `WelcomeEmail.html`  
**Features:**
- Warm welcome message
- Quick start guide
- Community guidelines link
- Explore button

**Usage:**
```csharp
await _emailService.SendWelcomeEmailAsync(
    userId,
    userEmail,
    userName);
```

---

### 10. **Digest Emails** 📰
**Trigger:** Scheduled daily/weekly  
**Priority:** Low (7)  
**Template:** `DigestEmail.html`  
**Features:**
- Summary of activity
- Trending posts
- Unread notifications
- Community highlights

**Usage:**
```csharp
await _emailService.SendDailyDigestAsync(userId);
await _emailService.SendWeeklyDigestAsync(userId);
```

---

## 🗄️ Database Schema

### Enhanced Notification Table

```sql
CREATE TABLE [Notifications] (
    [NotificationId] int IDENTITY(1,1) PRIMARY KEY,
    [UserId] nvarchar(450) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Message] nvarchar(500),
    [EntityType] nvarchar(50),
    [EntityId] nvarchar(450),
    [IsRead] bit DEFAULT 0,
    [CreatedAt] datetime2 DEFAULT GETDATE(),
    
    -- NEW COLUMNS FOR ENHANCED NOTIFICATIONS
    [ActorUserId] nvarchar(450),           -- Who performed the action
    [ActorDisplayName] nvarchar(100),      -- Actor's display name
    [ActorAvatarUrl] nvarchar(2048),       -- Actor's avatar
    [Url] nvarchar(2048),                  -- Direct link to the entity
    [EmailSent] bit DEFAULT 0,             -- Was email sent?
    [EmailSentAt] datetime2,               -- When was email sent?
    [ReadAt] datetime2,                    -- When was it read?
    [GroupId] nvarchar(100),               -- For grouping similar notifications
    
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]),
    FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers]([Id])
);

CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
CREATE INDEX IX_Notifications_ActorUserId ON Notifications(ActorUserId);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
CREATE INDEX IX_Notifications_UserId_Type_IsRead ON Notifications(UserId, Type, IsRead);
```

### Email Queue Table

```sql
CREATE TABLE [EmailQueues] (
    [EmailQueueId] int IDENTITY(1,1) PRIMARY KEY,
    [ToEmail] nvarchar(256) NOT NULL,
    [ToName] nvarchar(100),
    [Subject] nvarchar(200) NOT NULL,
    [HtmlBody] nvarchar(MAX) NOT NULL,
    [PlainTextBody] nvarchar(MAX),
    [Status] nvarchar(50) DEFAULT 'pending',
    [Priority] int DEFAULT 5,
    [Attempts] int DEFAULT 0,
    [MaxAttempts] int DEFAULT 3,
    [CreatedAt] datetime2 DEFAULT GETDATE(),
    [ScheduledFor] datetime2,
    [SentAt] datetime2,
    [ErrorMessage] nvarchar(MAX),
    [EmailType] nvarchar(50),
    [UserId] nvarchar(450),
    [NotificationId] int
);

CREATE INDEX IX_EmailQueues_Status_Priority ON EmailQueues(Status, Priority);
CREATE INDEX IX_EmailQueues_ScheduledFor ON EmailQueues(ScheduledFor);
```

### Notification Preferences Table

```sql
CREATE TABLE [NotificationPreferences] (
    [PreferenceId] int IDENTITY(1,1) PRIMARY KEY,
    [UserId] nvarchar(450) NOT NULL,
    [NotificationType] nvarchar(50) NOT NULL,
    [WebEnabled] bit DEFAULT 1,
    [EmailEnabled] bit DEFAULT 1,
    [PushEnabled] bit DEFAULT 0,
    [EmailFrequency] nvarchar(20) DEFAULT 'instant',
    [CreatedAt] datetime2 DEFAULT GETDATE(),
    [UpdatedAt] datetime2 DEFAULT GETDATE(),
    
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]),
    UNIQUE (UserId, NotificationType)
);
```

### User Notification Settings Table

```sql
CREATE TABLE [UserNotificationSettings] (
    [UserId] nvarchar(450) PRIMARY KEY,
    [EmailNotificationsEnabled] bit DEFAULT 1,
    [WebNotificationsEnabled] bit DEFAULT 1,
    [PushNotificationsEnabled] bit DEFAULT 0,
    [EmailDigestFrequency] nvarchar(20) DEFAULT 'never',
    [QuietHoursEnabled] bit DEFAULT 0,
    [QuietHoursStart] time,
    [QuietHoursEnd] time,
    [GroupNotifications] bit DEFAULT 1,
    [ShowNotificationPreviews] bit DEFAULT 1,
    [PlayNotificationSound] bit DEFAULT 0,
    [UnsubscribeFromAll] bit DEFAULT 0,
    [CreatedAt] datetime2 DEFAULT GETDATE(),
    [UpdatedAt] datetime2 DEFAULT GETDATE(),
    
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
);
```

---

## 📧 Email Templates

All templates are located in `EmailTemplates/` directory:

| Template | Purpose | Variables |
|----------|---------|-----------|
| `_EmailLayout.html` | Base layout | `{CONTENT}`, `{BASE_URL}`, `{UNSUBSCRIBE_URL}` |
| `CommentNotification.html` | New comments | `{ACTOR_NAME}`, `{COMMENT_CONTENT}`, `{POST_TITLE}` |
| `ReplyNotification.html` | Comment replies | `{ACTOR_NAME}`, `{YOUR_COMMENT}`, `{REPLY_CONTENT}` |
| `FollowNotification.html` | New followers | `{ACTOR_NAME}`, `{ACTOR_BIO}`, `{PROFILE_URL}` |
| `MentionNotification.html` | User mentions | `{ACTOR_NAME}`, `{MENTION_CONTENT}`, `{POST_TITLE}` |
| `DirectMessageNotification.html` | Private messages | `{ACTOR_NAME}`, `{MESSAGE_PREVIEW}`, `{CHAT_URL}` |
| `AnnouncementNotification.html` | Site announcements | `{TITLE}`, `{MESSAGE}`, `{LINK_URL}` |
| `UpvoteNotification.html` | Upvotes | `{ACTOR_NAME}`, `{ENTITY_TYPE}`, `{ENTITY_TITLE}` |
| `MilestoneNotification.html` | Milestones | `{MILESTONE_TITLE}`, `{VALUE}`, `{USER_STATS}` |
| `WelcomeEmail.html` | New users | `{USER_NAME}`, `{BASE_URL}` |
| `DigestEmail.html` | Daily/weekly digest | `{DIGEST_ITEMS}`, `{TRENDING_POSTS}` |

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "zubair007tanoli@gmail.com",
    "Password": "Sudo@Linux@334",
    "FromEmail": "no-reply@discussionspot.com",
    "FromName": "DiscussionSpot",
    "EnableSsl": true,
    "EnableEmails": true,
    "TestMode": false,
    "AdminEmail": "zubair007tanoli@gmail.com",
    "SendWelcomeEmail": true,
    "SendDigestEmails": true,
    "DigestTime": "09:00"
  }
}
```

### Email Settings Explained

| Setting | Description | Default |
|---------|-------------|---------|
| `SmtpHost` | SMTP server address | smtp.gmail.com |
| `SmtpPort` | SMTP port (587 for TLS, 465 for SSL) | 587 |
| `Username` | SMTP authentication username | - |
| `Password` | SMTP authentication password | - |
| `FromEmail` | Sender email address | no-reply@domain.com |
| `FromName` | Sender display name | DiscussionSpot |
| `EnableSsl` | Use SSL/TLS encryption | true |
| `EnableEmails` | Master switch for all emails | true |
| `TestMode` | Send all emails to admin (testing) | false |
| `AdminEmail` | Admin email for notifications | - |
| `SendWelcomeEmail` | Send welcome email on signup | true |
| `SendDigestEmails` | Enable digest emails | true |
| `DigestTime` | Time to send daily digests (24h format) | 09:00 |

---

## 👤 User Preferences

Users can control their notifications at `/notifications/settings`

### Preference Options

1. **Global Settings**
   - Enable/disable all email notifications
   - Enable/disable web notifications
   - Enable/disable push notifications
   - Unsubscribe from all notifications

2. **Email Digest**
   - Never (default)
   - Daily
   - Weekly

3. **Quiet Hours**
   - Enable quiet hours
   - Start time (e.g., 22:00)
   - End time (e.g., 08:00)

4. **Per-Notification Type**
   - Comment - Web/Email/Push
   - Reply - Web/Email/Push
   - Vote - Web/Email/Push
   - Follow - Web/Email/Push
   - Mention - Web/Email/Push
   - Message - Web/Email/Push
   - Award - Web/Email/Push
   - Community Post - Web/Email/Push
   - Milestone - Web/Email/Push
   - Announcement - Web/Email/Push

5. **Display Options**
   - Group similar notifications
   - Show notification previews
   - Play notification sound

---

## 🧪 Testing

### Test Individual Email Types

```csharp
// In a controller or service
[HttpGet("test-email/{type}")]
public async Task<IActionResult> TestEmail(string type, string userId)
{
    var notification = new Notification
    {
        UserId = userId,
        Type = type,
        Title = $"Test {type} notification",
        Message = "This is a test message",
        ActorUserId = "test-actor-id",
        ActorDisplayName = "Test User",
        ActorAvatarUrl = "/images/avatar.png",
        Url = "/test-url"
    };

    switch (type)
    {
        case "comment":
            await _emailService.SendCommentNotificationEmailAsync(
                notification, "Test Post Title", "This is a test comment", "/post/test");
            break;
        
        case "message":
            await _emailService.SendDirectMessageEmailAsync(
                notification, "Test Sender", "Hello! This is a test message.", "/chat/test");
            break;
        
        case "announcement":
            await _emailService.SendAnnouncementEmailAsync(
                notification, "Site Update", "We've added new features!", "/updates", "Learn More");
            break;
        
        // Add more cases as needed
    }

    return Ok("Test email queued");
}
```

### Test Email Queue Worker

```bash
# Check logs for email processing
tail -f logs/email-worker.log

# Check email queue status
SELECT Status, COUNT(*) as Count 
FROM EmailQueues 
GROUP BY Status;
```

### Test User Preferences

```csharp
// Check if user should receive email
var shouldSend = await _preferenceService.ShouldNotifyAsync(
    userId, 
    "comment", 
    "email");

Console.WriteLine($"Should send: {shouldSend}");
```

---

## 🔧 Troubleshooting

### Emails Not Sending

1. **Check Email Configuration**
   ```bash
   # Verify SMTP settings in appsettings.json
   # Test SMTP connection manually
   telnet smtp.gmail.com 587
   ```

2. **Check EnableEmails Flag**
   ```json
   "Email": {
     "EnableEmails": true  // Must be true
   }
   ```

3. **Check Email Queue**
   ```sql
   SELECT * FROM EmailQueues 
   WHERE Status = 'failed' 
   ORDER BY CreatedAt DESC;
   ```

4. **Check EmailWorkerService Logs**
   ```bash
   # Look for errors in application logs
   grep "EmailWorkerService" logs/*.log
   ```

### Emails Going to Spam

1. **Set up SPF Record**
   ```
   v=spf1 include:_spf.google.com ~all
   ```

2. **Set up DKIM**
   - Configure in Gmail/Outlook settings
   - Add DKIM DNS records

3. **Set up DMARC**
   ```
   v=DMARC1; p=none; rua=mailto:admin@yourdomain.com
   ```

4. **Use a Custom Domain**
   - Don't send from @gmail.com
   - Use no-reply@yourdomain.com

### User Not Receiving Emails

1. **Check User Preferences**
   ```sql
   SELECT * FROM UserNotificationSettings WHERE UserId = 'user-id';
   SELECT * FROM NotificationPreferences WHERE UserId = 'user-id';
   ```

2. **Check if User is Online**
   ```sql
   SELECT * FROM UserPresences 
   WHERE UserId = 'user-id' 
   AND LastSeen > DATEADD(MINUTE, -5, GETUTCDATE());
   ```
   *Note: Emails are NOT sent if user is online*

3. **Check Email Queue**
   ```sql
   SELECT * FROM EmailQueues 
   WHERE UserId = 'user-id' 
   ORDER BY CreatedAt DESC;
   ```

### Gmail App Password Issues

If using Gmail, you need an App Password:

1. Enable 2-Factor Authentication on your Google Account
2. Go to https://myaccount.google.com/apppasswords
3. Generate a new App Password for "Mail"
4. Use that password in appsettings.json

---

## 📊 Monitoring

### Key Metrics to Track

1. **Email Delivery Rate**
   ```sql
   SELECT 
       Status,
       COUNT(*) as Count,
       CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,2)) as Percentage
   FROM EmailQueues
   GROUP BY Status;
   ```

2. **Average Delivery Time**
   ```sql
   SELECT 
       AVG(DATEDIFF(SECOND, CreatedAt, SentAt)) as AvgDeliverySeconds
   FROM EmailQueues
   WHERE Status = 'sent';
   ```

3. **Failure Rate by Type**
   ```sql
   SELECT 
       EmailType,
       COUNT(*) as FailedCount
   FROM EmailQueues
   WHERE Status = 'failed'
   GROUP BY EmailType
   ORDER BY FailedCount DESC;
   ```

4. **User Engagement**
   ```sql
   SELECT 
       COUNT(DISTINCT UserId) as UsersReceivingEmails,
       COUNT(*) as TotalEmails,
       AVG(CAST(Attempts as FLOAT)) as AvgAttempts
   FROM EmailQueues
   WHERE CreatedAt > DATEADD(DAY, -7, GETUTCDATE());
   ```

---

## 🚀 Future Enhancements

- [ ] A/B testing for email templates
- [ ] Email open tracking (pixel)
- [ ] Link click tracking
- [ ] Unsubscribe link management
- [ ] Email template editor in admin panel
- [ ] Multi-language email support
- [ ] Push notifications for mobile
- [ ] SMS notifications
- [ ] Slack/Discord integrations
- [ ] Email preview before sending
- [ ] Scheduled announcement emails
- [ ] Auto-reply detection
- [ ] Email bounce handling

---

## 📝 Summary

✅ **10 notification types** fully implemented  
✅ **11 email templates** with responsive design  
✅ **4 new database tables** for queuing and preferences  
✅ **8 new columns** added to Notifications table  
✅ **Background worker** for reliable email sending  
✅ **User preference system** with fine-grained control  
✅ **Smart offline detection** to avoid email spam  
✅ **Priority-based queuing** for important notifications  
✅ **Retry logic** for failed deliveries  
✅ **Full SMTP support** with Gmail/Outlook  

---

## 📞 Support

If you encounter any issues:

1. Check this documentation
2. Review application logs
3. Check database EmailQueues table
4. Verify SMTP settings
5. Test with TestMode enabled

---

**Last Updated:** October 28, 2025  
**Version:** 1.0  
**Author:** AI Assistant  
**Status:** ✅ PRODUCTION READY

