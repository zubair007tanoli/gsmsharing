using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(
        string userId,
        string message,
        int postId,
        string type)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "New Activity",
            Message = message,
            EntityType = "post",
            EntityId = postId.ToString(),
            Type = type,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, string? userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCount2Async(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<CancellationToken> GetUnreadCountAsync(string? userId)
    {
        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return new CancellationTokenSource(unreadCount).Token;
    }

    public Task CreateAwardNotificationAsync(int postId, int awardId, string fromUserId)
    {
        throw new NotImplementedException();
    }

    public Task CreateNotificationAsync(object userId, string v1, int postId, string v2)
    {
        throw new NotImplementedException();
    }
}