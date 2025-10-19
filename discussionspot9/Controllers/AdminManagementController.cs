using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize]
    [Route("admin/manage")]
    public class AdminManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminManagementController> _logger;
        private readonly IAdminService _adminService;

        public AdminManagementController(
            ApplicationDbContext context,
            ILogger<AdminManagementController> logger,
            IAdminService adminService)
        {
            _context = context;
            _logger = logger;
            _adminService = adminService;
        }
        
        private async Task<bool> IsCurrentUserAdmin()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;
            return await _adminService.IsUserSiteAdminAsync(userId);
        }

        // Users Management
        [HttpGet("users")]
        public async Task<IActionResult> Users(int page = 1, string search = "")
        {
            const int pageSize = 20;
            
            var query = _context.UserProfiles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.DisplayName.Contains(search) || u.Bio.Contains(search));
            }

            var totalUsers = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.KarmaPoints)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get user activities count
            var userActivities = new Dictionary<string, int>();
            foreach (var user in users)
            {
                var activityCount = await _context.UserActivities
                    .Where(a => a.UserId == user.UserId && a.ActivityAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync();
                userActivities[user.UserId] = activityCount;
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.UserActivities = userActivities;
            ViewBag.TotalUsers = totalUsers;

            return View(users);
        }

        // Posts Management
        [HttpGet("posts")]
        public async Task<IActionResult> Posts(int page = 1, string search = "", string status = "all")
        {
            const int pageSize = 20;
            
            var query = _context.Posts
                .Include(p => p.Community)
                .Include(p => p.UserProfile)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            if (status != "all")
            {
                query = query.Where(p => p.Status == status);
            }

            var totalPosts = await query.CountAsync();
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get performance metrics
            var postMetrics = new Dictionary<int, (int Views, decimal Revenue)>();
            foreach (var post in posts)
            {
                var metrics = await _context.PostPerformanceMetrics
                    .Where(m => m.PostId == post.PostId && m.Date >= DateTime.UtcNow.AddDays(-30))
                    .ToListAsync();

                var revenue = await _context.AdSenseRevenues
                    .Where(a => a.PostId == post.PostId && a.Date >= DateTime.UtcNow.AddDays(-30))
                    .SumAsync(a => a.Earnings);

                postMetrics[post.PostId] = (metrics.Sum(m => m.Views), revenue);
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PostMetrics = postMetrics;
            ViewBag.TotalPosts = totalPosts;

            return View(posts);
        }

        // Communities Management
        [HttpGet("communities")]
        public async Task<IActionResult> Communities()
        {
            var communities = await _context.Communities
                .Include(c => c.Category)
                .OrderByDescending(c => c.MemberCount)
                .ToListAsync();

            return View(communities);
        }

        // Analytics Overview
        [HttpGet("analytics")]
        public async Task<IActionResult> Analytics()
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            var model = new
            {
                // User stats
                TotalUsers = await _context.UserProfiles.CountAsync(),
                ActiveUsers = await _context.UserActivities
                    .Where(a => a.ActivityAt >= DateTime.UtcNow.AddDays(-7))
                    .Select(a => a.UserId)
                    .Distinct()
                    .CountAsync(),
                
                // Content stats
                TotalPosts = await _context.Posts.CountAsync(),
                TotalComments = await _context.Comments.CountAsync(),
                TotalCommunities = await _context.Communities.CountAsync(),
                
                // Engagement
                RecentActivities = await _context.UserActivities
                    .Where(a => a.ActivityAt >= startDate)
                    .GroupBy(a => a.ActivityType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync(),
                
                // Top communities by activity
                TopCommunities = await _context.Communities
                    .OrderByDescending(c => c.PostCount)
                    .Take(10)
                    .Select(c => new { c.Name, c.PostCount, c.MemberCount })
                    .ToListAsync()
            };

            return View(model);
        }

        // Quick Post Actions
        [HttpPost("post/toggle-pin/{id}")]
        public async Task<IActionResult> TogglePin(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.IsPinned = !post.IsPinned;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = post.IsPinned ? "Post pinned" : "Post unpinned";
            return RedirectToAction(nameof(Posts));
        }

        [HttpPost("post/toggle-lock/{id}")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.IsLocked = !post.IsLocked;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = post.IsLocked ? "Post locked" : "Post unlocked";
            return RedirectToAction(nameof(Posts));
        }

        [HttpPost("post/delete/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.Status = "deleted";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post deleted";
            return RedirectToAction(nameof(Posts));
        }
        
        // ===============================================
        // USER ROLE & BAN MANAGEMENT (NEW)
        // ===============================================
        
        /// <summary>
        /// Assign a site-wide role to a user
        /// </summary>
        [HttpPost("user/assign-role")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUserRole(string userId, string roleName)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized - SiteAdmin role required" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            var result = await _adminService.AssignSiteRoleAsync(userId, roleName, currentUserId);
            
            if (result)
            {
                _logger.LogInformation("Site role {RoleName} assigned to user {UserId} by {AdminId}", roleName, userId, currentUserId);
                return Json(new { success = true, message = $"Role '{roleName}' assigned successfully" });
            }
            
            return Json(new { success = false, message = "Failed to assign role" });
        }
        
        /// <summary>
        /// Remove a site-wide role from a user
        /// </summary>
        [HttpPost("user/remove-role")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserRole(string userId, string roleName)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized - SiteAdmin role required" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            var result = await _adminService.RemoveSiteRoleAsync(userId, roleName, currentUserId);
            
            if (result)
            {
                _logger.LogInformation("Site role {RoleName} removed from user {UserId} by {AdminId}", roleName, userId, currentUserId);
                return Json(new { success = true, message = $"Role '{roleName}' removed successfully" });
            }
            
            return Json(new { success = false, message = "Failed to remove role" });
        }
        
        /// <summary>
        /// Ban a user site-wide
        /// </summary>
        [HttpPost("user/ban")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId, string reason, int? banDurationDays, bool isPermanent)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized - SiteAdmin role required" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Ban reason is required" });
            }
            
            DateTime? expiresAt = null;
            if (!isPermanent && banDurationDays.HasValue && banDurationDays.Value > 0)
            {
                expiresAt = DateTime.UtcNow.AddDays(banDurationDays.Value);
            }
            
            var result = await _adminService.BanUserAsync(userId, currentUserId, reason, expiresAt, isPermanent);
            
            if (result)
            {
                _logger.LogInformation("User {UserId} banned by {AdminId}. Reason: {Reason}, Duration: {Duration}", 
                    userId, currentUserId, reason, isPermanent ? "Permanent" : $"{banDurationDays} days");
                return Json(new { success = true, message = "User banned successfully" });
            }
            
            return Json(new { success = false, message = "Failed to ban user (may already be banned)" });
        }
        
        /// <summary>
        /// Unban a user
        /// </summary>
        [HttpPost("user/unban")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string userId, string reason)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized - SiteAdmin role required" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Ban lifted by administrator";
            }
            
            var result = await _adminService.UnbanUserAsync(userId, currentUserId, reason);
            
            if (result)
            {
                _logger.LogInformation("User {UserId} unbanned by {AdminId}. Reason: {Reason}", userId, currentUserId, reason);
                return Json(new { success = true, message = "User unbanned successfully" });
            }
            
            return Json(new { success = false, message = "Failed to unban user (may not be banned)" });
        }
        
        /// <summary>
        /// Get user details for admin panel
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> UserDetail(string userId)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }
            
            var userDetail = await _adminService.GetUserDetailAsync(userId);
            if (userDetail == null)
            {
                return NotFound();
            }
            
            return View(userDetail);
        }
        
        /// <summary>
        /// Get moderation logs
        /// </summary>
        [HttpGet("moderation-logs")]
        public async Task<IActionResult> ModerationLogs(int page = 1)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }
            
            var logs = await _adminService.GetModerationLogsAsync(page, 50);
            ViewBag.CurrentPage = page;
            
            return View(logs);
        }
    }
}

