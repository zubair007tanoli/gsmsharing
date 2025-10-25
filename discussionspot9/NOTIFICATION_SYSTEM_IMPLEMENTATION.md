# Notification System Implementation Guide

## ✅ Feature Complete!

The application now has a **fully functional real-time notification system** that notifies users when:
1. ✅ Someone comments on their post
2. ✅ Someone replies to their comment
3. ✅ Someone upvotes their post
4. ✅ Someone upvotes their comment

---

## 🎉 What Was Implemented

### 1. Enhanced NotificationService (`Services/NotificationService.cs`)

**New Methods:**
- `NotifyPostCommentAsync()` - Notifies post author when someone comments
- `NotifyCommentReplyAsync()` - Notifies comment author when someone replies
- `NotifyPostVoteAsync()` - Notifies post author when someone upvotes (upvotes only)
- `NotifyCommentVoteAsync()` - Notifies comment author when someone upvotes (upvotes only)
- `CreateNotificationAsync()` - Generic notification creation
- `GetUnreadCountAsync()` - Get unread notification count
- `MarkAsReadAsync()` - Mark notifications as read

**Features:**
- ✨ **Real-time delivery** via SignalR
- 🛡️ **Spam prevention** - Won't notify for upvotes more than once every 5 minutes from same context
- 🔕 **Self-notification prevention** - Users don't get notified about their own actions
- 📬 **Persistent notifications** - Saved to database
- 🔗 **Direct links** - Each notification includes a direct link to the relevant content

---

### 2. Updated Services

#### CommentService
- Integrated `INotificationService` via dependency injection
- **On Comment Creation:**
  - If it's a reply → Notify parent comment author
  - If it's a new comment → Notify post author
- **On Comment Upvote:**
  - Notify comment author about the upvote

#### PostService
- Already had `INotificationService` injected
- **On Post Upvote:**
  - Notify post author about the upvote

---

### 3. Interface Updates (`Interfaces/INotificationService.cs`)

Complete interface with all notification methods:

```csharp
public interface INotificationService
{
    // Comment notifications
    Task NotifyPostCommentAsync(int postId, int commentId, string commenterUserId, string commenterName);
    Task NotifyCommentReplyAsync(int parentCommentId, int replyCommentId, string replierUserId, string replierName);
    
    // Vote notifications
    Task NotifyPostVoteAsync(int postId, string voterUserId, string voterName, int voteType);
    Task NotifyCommentVoteAsync(int commentId, string voterUserId, string voterName, int voteType);
    
    // Generic & utility
    Task CreateNotificationAsync(string userId, string title, string message, string entityType, string entityId, string type);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId, string? userId);
    Task CreateAwardNotificationAsync(int postId, int awardId, string fromUserId);
}
```

---

## 🔔 Notification Types

| Type | Title | When It's Sent | Who Gets Notified |
|------|-------|----------------|-------------------|
| `comment` | "New Comment" | User comments on a post | Post author |
| `reply` | "New Reply" | User replies to a comment | Comment author |
| `upvote` (post) | "Post Upvoted" | User upvotes a post | Post author |
| `upvote` (comment) | "Comment Upvoted" | User upvotes a comment | Comment author |
| `award` | "Award Received!" | User gives an award | Post/Comment author |

---

## 🚀 How It Works

### Example Flow: User Comments on a Post

1. **User submits comment** via `CommentController.Create()`
2. **CommentService.CreateCommentAsync()** is called
   - Creates comment in database
   - Updates post comment count
   - **Gets commenter's display name** from UserProfile
   - **Calls `NotificationService.NotifyPostCommentAsync()`**
3. **NotificationService** does:
   - Fetches post and community data
   - Checks if commenter is the post author (skip if yes)
   - **Creates Notification record** in database
   - **Sends real-time SignalR message** to post author's notification group
4. **User receives notification:**
   - **Database**: Stored as unread notification
   - **Real-time**: SignalR push to browser (if online)
   - **Navbar badge**: Updated via `notification-handler.js`
   - **Toast notification**: Optional browser toast

---

## 📱 Real-time Delivery (SignalR)

### NotificationHub Configuration

Each notification is sent to a user-specific SignalR group:

```javascript
// Group name format
$"notifications-{userId}"
```

### Client-Side Integration

The client connects via `notification-handler.js`:

```javascript
connection.on("ReceiveNotification", (notification) => {
    // Update badge count
    // Show toast notification
    // Update notification list
});
```

---

## 🎯 Smart Notification Logic

### Spam Prevention
- **Vote notifications**: Only sent once every 5 minutes for the same post/comment
- Prevents notification flooding from rapid upvote/downvote changes

### Self-Notification Prevention
```csharp
// Don't notify if user is commenting on their own post
if (post.UserId == commenterUserId) return;

// Don't notify if user is voting on their own content
if (comment.UserId == voterUserId) return;
```

### Only Positive Interactions
- Only **upvotes** trigger notifications
- **Downvotes** do NOT notify (to prevent negativity)

---

## 🗄️ Database Schema

### Notifications Table
```sql
CREATE TABLE Notifications (
    NotificationId INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,          -- Who receives the notification
    Title NVARCHAR(200) NOT NULL,            -- "New Comment"
    Message NVARCHAR(MAX) NOT NULL,          -- "John commented on your post..."
    EntityType NVARCHAR(50),                 -- "comment", "post"
    EntityId NVARCHAR(100),                  -- commentId or postId
    Type NVARCHAR(50),                       -- "comment", "reply", "upvote"
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
```

---

## 🧪 Testing Guide

### Test 1: Comment Notifications

1. **User A**: Create a post
2. **User B**: Comment on User A's post
3. **Expected Result:**
   - User A receives notification: "User B commented on your post..."
   - Real-time badge update
   - Notification saved in database

### Test 2: Reply Notifications

1. **User A**: Comment on any post
2. **User B**: Reply to User A's comment
3. **Expected Result:**
   - User A receives notification: "User B replied to your comment"
   - Real-time update

### Test 3: Vote Notifications

1. **User A**: Create a post or comment
2. **User B**: Upvote it
3. **Expected Result:**
   - User A receives notification: "User B upvoted your post/comment"
   - No duplicate notifications for 5 minutes

### Test 4: Self-Notification Prevention

1. **User A**: Comment on their own post
2. **Expected Result:**
   - NO notification sent

### Test 5: SignalR Real-time

1. Open two browsers (or incognito)
2. Login as User A in Browser 1
3. Login as User B in Browser 2
4. User B comments on User A's post
5. **Expected Result:**
   - User A's notification badge updates **instantly** (no page refresh needed)

---

## 🔧 Configuration

### Required Services in `Program.cs`

```csharp
// Already configured:
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notification");
```

### Client-Side Scripts

**Already included** in `_Layout.cshtml`:
- `~/js/notification-handler.js` - SignalR connection and notification handling
- SignalR library automatically loaded

---

## 📊 Notification Flow Diagram

```
┌─────────────────┐
│  User Action    │ (Comment, Vote, etc.)
└────────┬────────┘
         ↓
┌────────────────────┐
│  Service Layer     │ (CommentService, PostService)
└────────┬───────────┘
         ↓
┌────────────────────────────┐
│  NotificationService       │
├────────────────────────────┤
│ 1. Check if should notify  │
│ 2. Create DB notification  │
│ 3. Send SignalR message    │
└────────┬───────────────────┘
         ↓
    ┌───┴────┐
    ↓        ↓
┌────────┐  ┌──────────────────┐
│Database│  │  SignalR Hub     │
└────────┘  └────────┬─────────┘
                     ↓
            ┌─────────────────┐
            │ User's Browser  │
            │ (Real-time)     │
            └─────────────────┘
```

---

## 🐛 Troubleshooting

### Notifications Not Appearing

**Check:**
1. Is `INotificationService` registered in `Program.cs`?
2. Is SignalR Hub mapped correctly?
3. Is the user authenticated?
4. Is `notification-handler.js` loaded and connected?
5. Check browser console for SignalR connection errors

**Debug Commands:**
```javascript
// In browser console
console.log(connection.state); // Should be "Connected"
```

### Duplicate Notifications

- Check spam prevention is working (5-minute window)
- Ensure notification service isn't injected multiple times
- Check for duplicate SignalR group subscriptions

### Database Errors

```sql
-- Verify Notifications table exists
SELECT TOP 10 * FROM Notifications ORDER BY CreatedAt DESC;

-- Check for orphaned notifications
SELECT COUNT(*) FROM Notifications WHERE UserId NOT IN (SELECT Id FROM AspNetUsers);
```

---

## 📝 Next Steps / Future Enhancements

Potential improvements:
- [ ] Email notifications for offline users
- [ ] Notification preferences (toggle types on/off)
- [ ] Batch notifications (digest mode)
- [ ] Push notifications (browser push API)
- [ ] Mark all as read functionality
- [ ] Notification grouping (e.g., "John and 5 others upvoted your post")
- [ ] Notification history/archive

---

## 🎓 Key Files Modified

| File | Purpose | Changes |
|------|---------|---------|
| `Services/NotificationService.cs` | Core notification logic | Complete rewrite with all notification types |
| `Interfaces/INotificationService.cs` | Service contract | Added all notification methods |
| `Services/CommentService.cs` | Comment operations | Added comment & reply notifications |
| `Services/PostService.cs` | Post operations | Added post vote notifications |
| `Hubs/NotificationHub.cs` | SignalR hub | Already configured for notifications |

---

## ✅ Summary

**The notification system is now FULLY FUNCTIONAL!**

Users will receive **instant, real-time notifications** for:
- Comments on their posts
- Replies to their comments  
- Upvotes on their posts and comments

All notifications are:
- ✅ Stored persistently in the database
- ✅ Delivered in real-time via SignalR
- ✅ Smart (no self-notifications, spam prevention)
- ✅ User-friendly (includes direct links)

**No additional configuration needed - it's ready to use!** 🎉

