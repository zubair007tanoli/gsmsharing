using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using GsmsharingV2.Hubs;

namespace GsmsharingV2.Services
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

        public async Task CreateNotificationAsync(string userId, string title, string content, string type, int? referenceId = null, string referenceType = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                ReferenceID = referenceId,
                ReferenceType = referenceType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification via SignalR
            await _notificationHub.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                NotificationID = notification.NotificationID,
                Title = title,
                Content = content,
                Type = type,
                CreatedAt = notification.CreatedAt
            });
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && (n.IsRead == false || n.IsRead == null))
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && (n.IsRead == false || n.IsRead == null));
        }
    }
}

