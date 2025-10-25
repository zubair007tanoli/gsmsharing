using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationHub> _logger;
    private readonly ApplicationDbContext _context;

    public NotificationHub(INotificationService notificationService,
                          ILogger<NotificationHub> logger,
                          ApplicationDbContext context)
    {
        _notificationService = notificationService;
        _logger = logger;
        _context = context;
    }

    public async Task JoinNotificationGroup()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications-{userId}");
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

    public async Task MarkNotificationAsRead(int notificationId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return;

        try
        {
            await _notificationService.MarkAsReadAsync(notificationId, userId);
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            await Clients.Caller.SendAsync("NotificationError", "Failed to mark as read");
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications-{userId}");
            
            // Auto-join admin group if user is admin
            var isAdmin = await _context.SiteRoles
                .AnyAsync(r => r.UserId == userId && r.RoleName == "SiteAdmin" && r.IsActive);
            
            if (isAdmin)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin-notifications");
                _logger.LogInformation($"Admin user {userId} joined admin-notifications group");
            }
            
            //var count = await _notificationService.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("UnreadNotificationCount", 1);
        }
        await base.OnConnectedAsync();
    }
    
    /// <summary>
    /// Manually join admin notification group
    /// </summary>
    public async Task JoinAdminGroup()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var isAdmin = await _context.SiteRoles
            .AnyAsync(r => r.UserId == userId && r.RoleName == "SiteAdmin" && r.IsActive);

        if (isAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin-notifications");
            _logger.LogInformation($"User {userId} joined admin-notifications group");
        }
    }
    
    /// <summary>
    /// Send notification to all admins
    /// </summary>
    public async Task SendAdminNotification(string title, string message, string type, string? url = null)
    {
        var userId = GetUserId();
        _logger.LogInformation($"Admin notification sent by {userId}: {title}");
        
        await Clients.Group("admin-notifications").SendAsync("ReceiveAdminNotification", new
        {
            title = title,
            message = message,
            type = type,
            url = url,
            timestamp = DateTime.UtcNow
        });
    }

    private string? GetUserId() =>
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}