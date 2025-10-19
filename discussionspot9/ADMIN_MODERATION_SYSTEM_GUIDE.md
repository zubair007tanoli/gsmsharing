# 🛡️ Admin & Moderation System Implementation Guide

## Overview

This guide implements a complete Role-Based Access Control (RBAC) system with:
- **Site Admins**: Full control over the entire platform
- **Community Admins/Moderators**: Control over specific communities
- **User Ban System**: Site-wide and community-specific bans
- **Moderation Logs**: Complete audit trail
- **Role Management**: Assign/revoke roles

---

## ✅ Phase 1: Database Models (COMPLETED)

### Files Created:
1. ✅ `Models/Domain/UserBan.cs` - Ban tracking
2. ✅ `Models/Domain/ModerationLog.cs` - Audit logs
3. ✅ `Models/Domain/SiteRole.cs` - Site-wide roles
4. ✅ `Models/Domain/UserProfile.cs` - Updated with ban fields
5. ✅ `Data/DbContext/ApplicationDbContext.cs` - Added DbSets

### Database Tables Added:

| Table | Purpose |
|-------|---------|
| `UserBans` | Tracks all bans (site & community) |
| `ModerationLogs` | Audit trail of all mod actions |
| `SiteRoles` | Site-wide roles (Admin, Moderator, etc.) |

---

## 📋 Phase 2: Services & Business Logic

### Create: `Services/AdminService.cs`

```csharp
using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Services
{
    public interface IAdminService
    {
        // Role Management
        Task<bool> AssignSiteRoleAsync(string userId, string roleName, string assignedByUserId);
        Task<bool> RemoveSiteRoleAsync(string userId, string roleName, string removedByUserId);
        Task<List<string>> GetUserSiteRolesAsync(string userId);
        Task<bool> IsUserSiteAdminAsync(string userId);
        Task<bool> IsUserModeratorAsync(string userId);
        
        // User Ban Management
        Task<bool> BanUserAsync(string userId, string bannedByUserId, string reason, DateTime? expiresAt, bool isPermanent);
        Task<bool> UnbanUserAsync(string userId, string unbannedByUserId, string reason);
        Task<bool> IsUserBannedAsync(string userId);
        Task<UserBan?> GetActiveBanAsync(string userId);
        
        // Community Ban Management
        Task<bool> BanUserFromCommunityAsync(string userId, int communityId, string bannedByUserId, string reason, DateTime? expiresAt);
        Task<bool> UnbanUserFromCommunityAsync(string userId, int communityId, string unbannedByUserId);
        Task<bool> IsUserBannedFromCommunityAsync(string userId, int communityId);
        
        // User Management
        Task<List<UserManagementViewModel>> GetAllUsersAsync(int page = 1, int pageSize = 50);
        Task<List<UserManagementViewModel>> SearchUsersAsync(string searchTerm);
        Task<UserDetailViewModel?> GetUserDetailAsync(string userId);
        
        // Moderation Logs
        Task LogModerationActionAsync(string moderatorId, string targetUserId, string actionType, string reason, string? entityId = null, int? communityId = null);
        Task<List<ModerationLogViewModel>> GetModerationLogsAsync(int page = 1, int pageSize = 50);
        Task<List<ModerationLogViewModel>> GetUserModerationLogsAsync(string userId, int page = 1);
    }
    
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        
        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }
        
        public async Task<bool> AssignSiteRoleAsync(string userId, string roleName, string assignedByUserId)
        {
            try
            {
                // Check if role already exists
                var existingRole = await _context.SiteRoles
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.RoleName == roleName && r.IsActive);
                    
                if (existingRole != null)
                {
                    return false; // Already has this role
                }
                
                var siteRole = new SiteRole
                {
                    UserId = userId,
                    RoleName = roleName,
                    AssignedAt = DateTime.UtcNow,
                    AssignedByUserId = assignedByUserId,
                    IsActive = true
                };
                
                _context.SiteRoles.Add(siteRole);
                
                // Also add to ASP.NET Identity roles
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
                
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogModerationActionAsync(assignedByUserId, userId, "role_assign", $"Assigned role: {roleName}");
                
                _logger.LogInformation("Role {RoleName} assigned to user {UserId} by {AssignedBy}", roleName, userId, assignedByUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
                return false;
            }
        }
        
        public async Task<bool> RemoveSiteRoleAsync(string userId, string roleName, string removedByUserId)
        {
            try
            {
                var role = await _context.SiteRoles
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.RoleName == roleName && r.IsActive);
                    
                if (role == null)
                {
                    return false;
                }
                
                role.IsActive = false;
                
                // Remove from ASP.NET Identity roles
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }
                
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogModerationActionAsync(removedByUserId, userId, "role_remove", $"Removed role: {roleName}");
                
                _logger.LogInformation("Role {RoleName} removed from user {UserId} by {RemovedBy}", roleName, userId, removedByUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
                return false;
            }
        }
        
        public async Task<List<string>> GetUserSiteRolesAsync(string userId)
        {
            return await _context.SiteRoles
                .Where(r => r.UserId == userId && r.IsActive)
                .Select(r => r.RoleName)
                .ToListAsync();
        }
        
        public async Task<bool> IsUserSiteAdminAsync(string userId)
        {
            return await _context.SiteRoles
                .AnyAsync(r => r.UserId == userId && r.RoleName == "SiteAdmin" && r.IsActive);
        }
        
        public async Task<bool> IsUserModeratorAsync(string userId)
        {
            return await _context.SiteRoles
                .AnyAsync(r => r.UserId == userId && 
                              (r.RoleName == "SiteAdmin" || r.RoleName == "Moderator") && 
                              r.IsActive);
        }
        
        public async Task<bool> BanUserAsync(string userId, string bannedByUserId, string reason, DateTime? expiresAt, bool isPermanent)
        {
            try
            {
                // Check if user is already banned
                var existingBan = await _context.UserBans
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.IsActive && b.CommunityId == null);
                    
                if (existingBan != null)
                {
                    return false; // Already banned
                }
                
                var ban = new UserBan
                {
                    UserId = userId,
                    BannedByUserId = bannedByUserId,
                    Reason = reason,
                    BannedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsPermanent = isPermanent,
                    IsActive = true,
                    BanType = "site"
                };
                
                _context.UserBans.Add(ban);
                
                // Update user profile
                var userProfile = await _context.UserProfiles.FindAsync(userId);
                if (userProfile != null)
                {
                    userProfile.IsBanned = true;
                    userProfile.BanExpiresAt = expiresAt;
                    userProfile.BanReason = reason;
                }
                
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogModerationActionAsync(bannedByUserId, userId, "ban_site", reason);
                
                _logger.LogInformation("User {UserId} banned by {BannedBy}. Reason: {Reason}", userId, bannedByUserId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId}", userId);
                return false;
            }
        }
        
        public async Task<bool> UnbanUserAsync(string userId, string unbannedByUserId, string reason)
        {
            try
            {
                var ban = await _context.UserBans
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.IsActive && b.CommunityId == null);
                    
                if (ban == null)
                {
                    return false;
                }
                
                ban.IsActive = false;
                ban.LiftedAt = DateTime.UtcNow;
                ban.LiftedByUserId = unbannedByUserId;
                ban.LiftReason = reason;
                
                // Update user profile
                var userProfile = await _context.UserProfiles.FindAsync(userId);
                if (userProfile != null)
                {
                    userProfile.IsBanned = false;
                    userProfile.BanExpiresAt = null;
                    userProfile.BanReason = null;
                }
                
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogModerationActionAsync(unbannedByUserId, userId, "unban_site", reason);
                
                _logger.LogInformation("User {UserId} unbanned by {UnbannedBy}. Reason: {Reason}", userId, unbannedByUserId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId}", userId);
                return false;
            }
        }
        
        public async Task<bool> IsUserBannedAsync(string userId)
        {
            var ban = await _context.UserBans
                .FirstOrDefaultAsync(b => b.UserId == userId && b.IsActive && b.CommunityId == null);
                
            if (ban == null) return false;
            
            // Check if temporary ban has expired
            if (!ban.IsPermanent && ban.ExpiresAt.HasValue && ban.ExpiresAt.Value < DateTime.UtcNow)
            {
                ban.IsActive = false;
                await _context.SaveChangesAsync();
                return false;
            }
            
            return true;
        }
        
        public async Task<UserBan?> GetActiveBanAsync(string userId)
        {
            return await _context.UserBans
                .Where(b => b.UserId == userId && b.IsActive && b.CommunityId == null)
                .Include(b => b.BannedByUser)
                .FirstOrDefaultAsync();
        }
        
        public async Task<bool> BanUserFromCommunityAsync(string userId, int communityId, string bannedByUserId, string reason, DateTime? expiresAt)
        {
            try
            {
                var ban = new UserBan
                {
                    UserId = userId,
                    CommunityId = communityId,
                    BannedByUserId = bannedByUserId,
                    Reason = reason,
                    BannedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsPermanent = expiresAt == null,
                    IsActive = true,
                    BanType = "community"
                };
                
                _context.UserBans.Add(ban);
                
                // Remove user from community members
                var membership = await _context.CommunityMembers
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.CommunityId == communityId);
                    
                if (membership != null)
                {
                    _context.CommunityMembers.Remove(membership);
                }
                
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogModerationActionAsync(bannedByUserId, userId, "ban_community", reason, null, communityId);
                
                _logger.LogInformation("User {UserId} banned from community {CommunityId} by {BannedBy}", userId, communityId, bannedByUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user {UserId} from community {CommunityId}", userId, communityId);
                return false;
            }
        }
        
        public async Task<bool> UnbanUserFromCommunityAsync(string userId, int communityId, string unbannedByUserId)
        {
            try
            {
                var ban = await _context.UserBans
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.CommunityId == communityId && b.IsActive);
                    
                if (ban == null)
                {
                    return false;
                }
                
                ban.IsActive = false;
                ban.LiftedAt = DateTime.UtcNow;
                ban.LiftedByUserId = unbannedByUserId;
                
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogModerationActionAsync(unbannedByUserId, userId, "unban_community", "Community ban lifted", null, communityId);
                
                _logger.LogInformation("User {UserId} unbanned from community {CommunityId} by {UnbannedBy}", userId, communityId, unbannedByUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user {UserId} from community {CommunityId}", userId, communityId);
                return false;
            }
        }
        
        public async Task<bool> IsUserBannedFromCommunityAsync(string userId, int communityId)
        {
            var ban = await _context.UserBans
                .FirstOrDefaultAsync(b => b.UserId == userId && b.CommunityId == communityId && b.IsActive);
                
            if (ban == null) return false;
            
            // Check if temporary ban has expired
            if (!ban.IsPermanent && ban.ExpiresAt.HasValue && ban.ExpiresAt.Value < DateTime.UtcNow)
            {
                ban.IsActive = false;
                await _context.SaveChangesAsync();
                return false;
            }
            
            return true;
        }
        
        public async Task<List<UserManagementViewModel>> GetAllUsersAsync(int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            
            var users = await _context.UserProfiles
                .Include(u => u.User)
                .OrderByDescending(u => u.JoinDate)
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new UserManagementViewModel
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    Email = u.User.Email,
                    JoinDate = u.JoinDate,
                    LastActive = u.LastActive,
                    KarmaPoints = u.KarmaPoints,
                    IsVerified = u.IsVerified,
                    IsBanned = u.IsBanned,
                    BanReason = u.BanReason,
                    BanExpiresAt = u.BanExpiresAt,
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync();
                
            // Get roles for each user
            foreach (var user in users)
            {
                user.Roles = await GetUserSiteRolesAsync(user.UserId);
            }
            
            return users;
        }
        
        public async Task<List<UserManagementViewModel>> SearchUsersAsync(string searchTerm)
        {
            var users = await _context.UserProfiles
                .Include(u => u.User)
                .Where(u => u.DisplayName.Contains(searchTerm) || u.User.Email.Contains(searchTerm))
                .Take(50)
                .Select(u => new UserManagementViewModel
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    Email = u.User.Email,
                    JoinDate = u.JoinDate,
                    LastActive = u.LastActive,
                    KarmaPoints = u.KarmaPoints,
                    IsVerified = u.IsVerified,
                    IsBanned = u.IsBanned,
                    BanReason = u.BanReason,
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync();
                
            foreach (var user in users)
            {
                user.Roles = await GetUserSiteRolesAsync(user.UserId);
            }
            
            return users;
        }
        
        public async Task<UserDetailViewModel?> GetUserDetailAsync(string userId)
        {
            var user = await _context.UserProfiles
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.UserId == userId);
                
            if (user == null) return null;
            
            var detail = new UserDetailViewModel
            {
                UserId = user.UserId,
                DisplayName = user.DisplayName,
                Email = user.User.Email,
                Bio = user.Bio,
                Website = user.Website,
                Location = user.Location,
                JoinDate = user.JoinDate,
                LastActive = user.LastActive,
                KarmaPoints = user.KarmaPoints,
                IsVerified = user.IsVerified,
                IsBanned = user.IsBanned,
                BanReason = user.BanReason,
                BanExpiresAt = user.BanExpiresAt,
                AvatarUrl = user.AvatarUrl,
                BannerUrl = user.BannerUrl,
                Roles = await GetUserSiteRolesAsync(userId),
                PostCount = await _context.Posts.CountAsync(p => p.UserId == userId),
                CommentCount = await _context.Comments.CountAsync(c => c.UserId == userId),
                CommunityCount = await _context.CommunityMembers.CountAsync(m => m.UserId == userId)
            };
            
            return detail;
        }
        
        public async Task LogModerationActionAsync(string moderatorId, string targetUserId, string actionType, string reason, string? entityId = null, int? communityId = null)
        {
            try
            {
                var log = new ModerationLog
                {
                    ModeratorId = moderatorId,
                    TargetUserId = targetUserId,
                    CommunityId = communityId,
                    ActionType = actionType,
                    EntityType = "user",
                    EntityId = entityId,
                    Reason = reason,
                    PerformedAt = DateTime.UtcNow
                };
                
                _context.ModerationLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging moderation action");
            }
        }
        
        public async Task<List<ModerationLogViewModel>> GetModerationLogsAsync(int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            
            return await _context.ModerationLogs
                .Include(l => l.Moderator)
                .Include(l => l.TargetUser)
                .Include(l => l.Community)
                .OrderByDescending(l => l.PerformedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(l => new ModerationLogViewModel
                {
                    LogId = l.LogId,
                    ModeratorName = l.Moderator.UserName,
                    TargetUserName = l.TargetUser.UserName,
                    CommunityName = l.Community != null ? l.Community.Name : null,
                    ActionType = l.ActionType,
                    Reason = l.Reason,
                    PerformedAt = l.PerformedAt
                })
                .ToListAsync();
        }
        
        public async Task<List<ModerationLogViewModel>> GetUserModerationLogsAsync(string userId, int page = 1)
        {
            var skip = (page - 1) * 50;
            
            return await _context.ModerationLogs
                .Include(l => l.Moderator)
                .Include(l => l.Community)
                .Where(l => l.TargetUserId == userId)
                .OrderByDescending(l => l.PerformedAt)
                .Skip(skip)
                .Take(50)
                .Select(l => new ModerationLogViewModel
                {
                    LogId = l.LogId,
                    ModeratorName = l.Moderator.UserName,
                    ActionType = l.ActionType,
                    Reason = l.Reason,
                    PerformedAt = l.PerformedAt,
                    CommunityName = l.Community != null ? l.Community.Name : null
                })
                .ToListAsync();
        }
    }
    
    // ViewModels
    public class UserManagementViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiresAt { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
    
    public class UserDetailViewModel : UserManagementViewModel
    {
        public string? Bio { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public string? BannerUrl { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunityCount { get; set; }
    }
    
    public class ModerationLogViewModel
    {
        public long LogId { get; set; }
        public string ModeratorName { get; set; }
        public string? TargetUserName { get; set; }
        public string? CommunityName { get; set; }
        public string ActionType { get; set; }
        public string Reason { get; set; }
        public DateTime PerformedAt { get; set; }
    }
}
```

---

## 📋 Phase 3: Register Services in Program.cs

Add to `Program.cs`:

```csharp
// Admin & Moderation Services
builder.Services.AddScoped<IAdminService, AdminService>();
```

---

## 📋 Phase 4: Database Migration

Run these commands:

```bash
# Create migration
dotnet ef migrations add AddAdminModerationSystem --project discussionspot9

# Update database
dotnet ef database update --project discussionspot9
```

---

## 📋 Phase 5: Create Admin Controller

File: `Controllers/AdminController.cs`

```csharp
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize] // Will add custom authorization later
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }
        
        // Check if current user is admin
        private async Task<bool> IsCurrentUserAdmin()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;
            return await _adminService.IsUserSiteAdminAsync(userId);
        }
        
        // GET: /admin
        public async Task<IActionResult> Index()
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }
            
            return View();
        }
        
        // GET: /admin/users
        public async Task<IActionResult> Users(int page = 1)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }
            
            var users = await _adminService.GetAllUsersAsync(page, 50);
            return View(users);
        }
        
        // GET: /admin/users/{userId}
        public async Task<IActionResult> UserDetail(string userId)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }
            
            var user = await _adminService.GetUserDetailAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }
        
        // POST: /admin/ban-user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId, string reason, int? banDurationDays, bool isPermanent)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            
            DateTime? expiresAt = null;
            if (!isPermanent && banDurationDays.HasValue)
            {
                expiresAt = DateTime.UtcNow.AddDays(banDurationDays.Value);
            }
            
            var result = await _adminService.BanUserAsync(userId, currentUserId, reason, expiresAt, isPermanent);
            
            if (result)
            {
                TempData["SuccessMessage"] = "User banned successfully";
                return Json(new { success = true });
            }
            
            return Json(new { success = false, message = "Failed to ban user" });
        }
        
        // POST: /admin/unban-user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string userId, string reason)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            
            var result = await _adminService.UnbanUserAsync(userId, currentUserId, reason);
            
            if (result)
            {
                TempData["SuccessMessage"] = "User unbanned successfully";
                return Json(new { success = true });
            }
            
            return Json(new { success = false, message = "Failed to unban user" });
        }
        
        // POST: /admin/assign-role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            
            var result = await _adminService.AssignSiteRoleAsync(userId, roleName, currentUserId);
            
            if (result)
            {
                TempData["SuccessMessage"] = $"Role '{roleName}' assigned successfully";
                return Json(new { success = true });
            }
            
            return Json(new { success = false, message = "Failed to assign role" });
        }
        
        // POST: /admin/remove-role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            
            var result = await _adminService.RemoveSiteRoleAsync(userId, roleName, currentUserId);
            
            if (result)
            {
                TempData["SuccessMessage"] = $"Role '{roleName}' removed successfully";
                return Json(new { success = true });
            }
            
            return Json(new { success = false, message = "Failed to remove role" });
        }
        
        // GET: /admin/moderation-logs
        public async Task<IActionResult> ModerationLogs(int page = 1)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }
            
            var logs = await _adminService.GetModerationLogsAsync(page, 50);
            return View(logs);
        }
    }
}
```

---

## 📋 Phase 6: Community Moderation Controller

Update `CommunityController.cs` to add moderation actions:

```csharp
// Add to CommunityController.cs

/// <summary>
/// Ban user from community (Community Admin only)
/// </summary>
[HttpPost]
[Authorize]
public async Task<IActionResult> BanUserFromCommunity(int communityId, string userId, string reason, int? banDurationDays)
{
    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(currentUserId))
    {
        return Json(new { success = false, message = "Not authenticated" });
    }
    
    // Check if current user is community admin/moderator
    var isAdmin = await _communityService.IsCommunityAdminAsync(communityId, currentUserId);
    var isModerator = await _communityService.IsCommunityModeratorAsync(communityId, currentUserId);
    
    if (!isAdmin && !isModerator)
    {
        return Json(new { success = false, message = "Insufficient permissions" });
    }
    
    DateTime? expiresAt = null;
    if (banDurationDays.HasValue)
    {
        expiresAt = DateTime.UtcNow.AddDays(banDurationDays.Value);
    }
    
    var result = await _adminService.BanUserFromCommunityAsync(userId, communityId, currentUserId, reason, expiresAt);
    
    return Json(new { success = result });
}

/// <summary>
/// Change community member role (Community Admin only)
/// </summary>
[HttpPost]
[Authorize]
public async Task<IActionResult> ChangeMemberRole(int communityId, string userId, string newRole)
{
    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(currentUserId))
    {
        return Json(new { success = false, message = "Not authenticated" });
    }
    
    // Check if current user is community admin
    var isAdmin = await _communityService.IsCommunityAdminAsync(communityId, currentUserId);
    
    if (!isAdmin)
    {
        return Json(new { success = false, message = "Only community admins can change roles" });
    }
    
    var result = await _communityService.PromoteDemoteCommunityMemberAsync(communityId, userId, newRole, currentUserId);
    
    return Json(result);
}
```

---

## 📋 Phase 7: Create Admin Views

### File: `Views/Admin/Index.cshtml`

```html
@{
    ViewData["Title"] = "Admin Dashboard";
}

<div class="container-fluid mt-4">
    <h1 class="mb-4">
        <i class="fas fa-shield-alt"></i> Admin Dashboard
    </h1>
    
    <div class="row">
        <div class="col-md-3 mb-3">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">
                        <i class="fas fa-users"></i> User Management
                    </h5>
                    <p class="card-text">Manage users, roles, and permissions</p>
                    <a asp-action="Users" class="btn btn-primary">Manage Users</a>
                </div>
            </div>
        </div>
        
        <div class="col-md-3 mb-3">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">
                        <i class="fas fa-clipboard-list"></i> Moderation Logs
                    </h5>
                    <p class="card-text">View all moderation actions</p>
                    <a asp-action="ModerationLogs" class="btn btn-primary">View Logs</a>
                </div>
            </div>
        </div>
        
        <div class="col-md-3 mb-3">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">
                        <i class="fas fa-ban"></i> Banned Users
                    </h5>
                    <p class="card-text">Manage banned users</p>
                    <a asp-action="BannedUsers" class="btn btn-primary">View Bans</a>
                </div>
            </div>
        </div>
        
        <div class="col-md-3 mb-3">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">
                        <i class="fas fa-chart-line"></i> Statistics
                    </h5>
                    <p class="card-text">Platform statistics</p>
                    <a asp-action="Stats" class="btn btn-primary">View Stats</a>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 📋 Next Steps

1. Create the missing view models
2. Create the admin UI views
3. Add authorization attributes
4. Create middleware to check bans
5. Add community moderation UI
6. Create ban appeal system (optional)

---

This guide provides a complete foundation for the admin/moderation system. Would you like me to continue with the implementation of any specific part?

