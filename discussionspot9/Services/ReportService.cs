using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class ReportService : IReportService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ReportService> _logger;
        private readonly IAdminService _adminService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ReportService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<ReportService> logger,
            IAdminService adminService,
            IHubContext<NotificationHub> notificationHub)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _adminService = adminService;
            _notificationHub = notificationHub;
        }

        public async Task<ServiceResult> CreateReportAsync(int postId, string userId, string reason, string? details)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Verify post exists
                var post = await context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return ServiceResult.ErrorResult("Post not found");
                }

                // Check if user already reported this post
                var existingReport = await context.PostReports
                    .AnyAsync(r => r.PostId == postId && r.UserId == userId);

                if (existingReport)
                {
                    return ServiceResult.ErrorResult("You have already reported this post");
                }

                // Create the report
                var report = new PostReport
                {
                    PostId = postId,
                    UserId = userId,
                    Reason = reason,
                    Details = details,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                context.PostReports.Add(report);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Report created: ReportId={report.ReportId}, PostId={postId}, UserId={userId}, Reason={reason}");

                // Notify all admins
                await NotifyAdminsAboutReportAsync(report.ReportId, post.Title, reason);

                return ServiceResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating report for post {postId}");
                return ServiceResult.ErrorResult("Failed to submit report");
            }
        }

        public async Task<List<PostReportViewModel>> GetPendingReportsAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var reports = await context.PostReports
                .Where(r => r.Status == "pending")
                .Include(r => r.Post)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new PostReportViewModel
                {
                    ReportId = r.ReportId,
                    PostId = r.PostId,
                    PostTitle = r.Post.Title,
                    PostUrl = $"/r/{r.Post.Community.Slug}/posts/{r.Post.Slug}",
                    ReporterName = r.User.UserName ?? "Unknown",
                    ReporterId = r.UserId,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByName = r.ReviewedBy != null ? r.ReviewedBy.UserName : null,
                    AdminNotes = r.AdminNotes
                })
                .ToListAsync();

            return reports;
        }

        public async Task<List<PostReportViewModel>> GetAllReportsAsync(string status = "all", int page = 1, int pageSize = 20)
        {
            using var context = _contextFactory.CreateDbContext();

            var query = context.PostReports
                .Include(r => r.Post)
                    .ThenInclude(p => p.Community)
                .Include(r => r.User)
                .Include(r => r.ReviewedBy)
                .AsQueryable();

            if (status != "all")
            {
                query = query.Where(r => r.Status == status);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new PostReportViewModel
                {
                    ReportId = r.ReportId,
                    PostId = r.PostId,
                    PostTitle = r.Post.Title,
                    PostUrl = $"/r/{r.Post.Community.Slug}/posts/{r.Post.Slug}",
                    ReporterName = r.User.UserName ?? "Unknown",
                    ReporterId = r.UserId,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByName = r.ReviewedBy != null ? r.ReviewedBy.UserName : null,
                    AdminNotes = r.AdminNotes
                })
                .ToListAsync();

            return reports;
        }

        public async Task<PostReportViewModel?> GetReportByIdAsync(int reportId)
        {
            using var context = _contextFactory.CreateDbContext();

            var report = await context.PostReports
                .Include(r => r.Post)
                    .ThenInclude(p => p.Community)
                .Include(r => r.User)
                .Include(r => r.ReviewedBy)
                .Where(r => r.ReportId == reportId)
                .Select(r => new PostReportViewModel
                {
                    ReportId = r.ReportId,
                    PostId = r.PostId,
                    PostTitle = r.Post.Title,
                    PostUrl = $"/r/{r.Post.Community.Slug}/posts/{r.Post.Slug}",
                    ReporterName = r.User.UserName ?? "Unknown",
                    ReporterId = r.UserId,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByName = r.ReviewedBy != null ? r.ReviewedBy.UserName : null,
                    AdminNotes = r.AdminNotes
                })
                .FirstOrDefaultAsync();

            return report;
        }

        public async Task<ServiceResult> UpdateReportStatusAsync(int reportId, string status, string adminUserId, string? adminNotes = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var report = await context.PostReports.FindAsync(reportId);
                if (report == null)
                {
                    return ServiceResult.ErrorResult("Report not found");
                }

                report.Status = status;
                report.ReviewedAt = DateTime.UtcNow;
                report.ReviewedByUserId = adminUserId;
                if (!string.IsNullOrEmpty(adminNotes))
                {
                    report.AdminNotes = adminNotes;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation($"Report {reportId} status updated to {status} by admin {adminUserId}");

                return ServiceResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating report status for report {reportId}");
                return ServiceResult.ErrorResult("Failed to update report status");
            }
        }

        public async Task<int> GetPendingReportCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PostReports.CountAsync(r => r.Status == "pending");
        }

        public async Task<ServiceResult> DismissReportAsync(int reportId, string adminUserId)
        {
            return await UpdateReportStatusAsync(reportId, "dismissed", adminUserId, "Report dismissed by admin");
        }

        public async Task<ServiceResult> ResolveReportAsync(int reportId, string adminUserId, string? action = null)
        {
            var notes = string.IsNullOrEmpty(action) ? "Report resolved" : $"Report resolved. Action taken: {action}";
            return await UpdateReportStatusAsync(reportId, "resolved", adminUserId, notes);
        }

        private async Task NotifyAdminsAboutReportAsync(int reportId, string postTitle, string reason)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Get all admin user IDs
                var adminUserIds = await _adminService.GetAllAdminUserIdsAsync();

                if (!adminUserIds.Any())
                {
                    _logger.LogWarning("No admin users found to notify about report");
                    return;
                }

                // Create notifications for each admin
                foreach (var adminUserId in adminUserIds)
                {
                    var notification = new Notification
                    {
                        UserId = adminUserId,
                        Type = "report",
                        Title = "New Post Report",
                        Message = $"Post reported: \"{postTitle}\" - Reason: {reason}",
                        EntityType = "report",
                        EntityId = reportId.ToString(),
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Notifications.Add(notification);
                }

                await context.SaveChangesAsync();

                // Send real-time SignalR notification to all admins
                foreach (var adminUserId in adminUserIds)
                {
                    await _notificationHub.Clients.Group($"notifications-{adminUserId}")
                        .SendAsync("ReceiveNotification", new
                        {
                            notificationId = reportId,
                            type = "report",
                            title = "New Post Report",
                            message = $"Post reported: \"{postTitle}\"",
                            url = $"/admin/manage/reports/{reportId}",
                            createdAt = DateTime.UtcNow
                        });
                }

                // Also broadcast to admin group
                await _notificationHub.Clients.Group("admin-notifications")
                    .SendAsync("ReceiveAdminNotification", new
                    {
                        type = "report",
                        title = "New Report Submitted",
                        message = $"Post: \"{postTitle}\" - Reason: {reason}",
                        reportId = reportId,
                        timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation($"Notified {adminUserIds.Count} admins about report {reportId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying admins about report {reportId}");
            }
        }
    }
}

