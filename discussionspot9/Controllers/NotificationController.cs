using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly INotificationPreferenceService _preferenceService;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            INotificationService notificationService,
            INotificationPreferenceService preferenceService,
            IEmailService emailService,
            ILogger<NotificationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _preferenceService = preferenceService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// GET: /notifications - Full notification center page
        /// </summary>
        [HttpGet]
        [Route("notifications")]
        public async Task<IActionResult> Index(string? type = null, bool? isRead = null, int page = 1)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                const int pageSize = 20;
                var query = _context.Notifications
                    .Where(n => n.UserId == userId);

                // Apply filters
                if (!string.IsNullOrEmpty(type))
                    query = query.Where(n => n.Type == type);

                if (isRead.HasValue)
                    query = query.Where(n => n.IsRead == isRead.Value);

                // Get total count
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Get paginated notifications
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentType = type;
                ViewBag.CurrentIsRead = isRead;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.UnreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications page");
                TempData["ErrorMessage"] = "Error loading notifications";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: /notifications/settings - Notification preferences page
        /// </summary>
        [HttpGet]
        [Route("notifications/settings")]
        public async Task<IActionResult> Settings()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var settings = await _preferenceService.GetUserSettingsAsync(userId);
                var preferences = await _preferenceService.GetUserPreferencesAsync(userId);

                ViewBag.Preferences = preferences;

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification settings");
                TempData["ErrorMessage"] = "Error loading settings";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: /notifications/settings - Update notification preferences
        /// </summary>
        [HttpPost]
        [Route("notifications/settings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(UserNotificationSettings settings)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                settings.UserId = userId;
                var success = await _preferenceService.UpdateUserSettingsAsync(userId, settings);

                if (success)
                {
                    TempData["SuccessMessage"] = "Notification preferences updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update preferences";
                }

                return RedirectToAction(nameof(Settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings");
                TempData["ErrorMessage"] = "Error updating settings";
                return RedirectToAction(nameof(Settings));
            }
        }

        /// <summary>
        /// POST: /api/notification/{id}/read - Mark notification as read
        /// </summary>
        [HttpPost]
        [Route("api/notification/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _notificationService.MarkAsReadAsync(id, userId);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return BadRequest(new { success = false, message = "Error marking as read" });
            }
        }

        /// <summary>
        /// POST: /api/notification/mark-all-read - Mark all notifications as read
        /// </summary>
        [HttpPost]
        [Route("api/notification/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, count = unreadNotifications.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all as read");
                return BadRequest(new { success = false });
            }
        }

        /// <summary>
        /// DELETE: /api/notification/{id} - Delete a notification
        /// </summary>
        [HttpDelete]
        [Route("api/notification/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

                if (notification == null)
                    return NotFound();

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return BadRequest(new { success = false });
            }
        }

        /// <summary>
        /// DELETE: /api/notification/clear-read - Delete all read notifications
        /// </summary>
        [HttpDelete]
        [Route("api/notification/clear-read")]
        public async Task<IActionResult> ClearReadNotifications()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var readNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsRead)
                    .ToListAsync();

                _context.Notifications.RemoveRange(readNotifications);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, count = readNotifications.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing read notifications");
                return BadRequest(new { success = false });
            }
        }

        /// <summary>
        /// POST: /api/notification/test - Send test notification
        /// </summary>
        [HttpPost]
        [Route("api/notification/test")]
        public async Task<IActionResult> SendTestNotification()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                await _notificationService.CreateNotificationAsync(
                    userId: userId,
                    title: "Test Notification",
                    message: "This is a test notification. If you see this, your notification system is working!",
                    entityType: "test",
                    entityId: "0",
                    type: "test");

                return Ok(new { success = true, message = "Test notification sent!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                return BadRequest(new { success = false });
            }
        }

        /// <summary>
        /// GET: /api/notification/unread-count - Get unread notification count
        /// </summary>
        [HttpGet]
        [Route("api/notification/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return BadRequest(new { count = 0 });
            }
        }
    }
}

