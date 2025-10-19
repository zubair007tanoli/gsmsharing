using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
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
        
        #region Role Management
        
        public async Task<bool> AssignSiteRoleAsync(string userId, string roleName, string assignedByUserId)
        {
            try
            {
                // Check if role already exists
                var existingRole = await _context.SiteRoles
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.RoleName == roleName && r.IsActive);
                    
                if (existingRole != null)
                {
                    _logger.LogWarning("User {UserId} already has role {RoleName}", userId, roleName);
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
                
                // Also add to ASP.NET Identity roles for built-in support
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && !await _userManager.IsInRoleAsync(user, roleName))
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
                    _logger.LogWarning("Role {RoleName} not found for user {UserId}", roleName, userId);
                    return false;
                }
                
                role.IsActive = false;
                
                // Remove from ASP.NET Identity roles
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && await _userManager.IsInRoleAsync(user, roleName))
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
        
        #endregion
        
        #region User Ban Management
        
        public async Task<bool> BanUserAsync(string userId, string bannedByUserId, string reason, DateTime? expiresAt, bool isPermanent)
        {
            try
            {
                // Check if user is already banned
                var existingBan = await _context.UserBans
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.IsActive && b.CommunityId == null);
                    
                if (existingBan != null)
                {
                    _logger.LogWarning("User {UserId} is already banned", userId);
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
                    _logger.LogWarning("No active ban found for user {UserId}", userId);
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
                
                // Update user profile
                var userProfile = await _context.UserProfiles.FindAsync(userId);
                if (userProfile != null)
                {
                    userProfile.IsBanned = false;
                    userProfile.BanExpiresAt = null;
                }
                
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
        
        #endregion
        
        #region Community Ban Management
        
        public async Task<bool> BanUserFromCommunityAsync(string userId, int communityId, string bannedByUserId, string reason, DateTime? expiresAt)
        {
            try
            {
                // Check if already banned from this community
                var existingBan = await _context.UserBans
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.CommunityId == communityId && b.IsActive);
                    
                if (existingBan != null)
                {
                    _logger.LogWarning("User {UserId} is already banned from community {CommunityId}", userId, communityId);
                    return false;
                }
                
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
                    
                    // Update member count
                    var community = await _context.Communities.FindAsync(communityId);
                    if (community != null)
                    {
                        community.MemberCount = Math.Max(0, community.MemberCount - 1);
                    }
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
                    _logger.LogWarning("No active community ban found for user {UserId} in community {CommunityId}", userId, communityId);
                    return false;
                }
                
                ban.IsActive = false;
                ban.LiftedAt = DateTime.UtcNow;
                ban.LiftedByUserId = unbannedByUserId;
                ban.LiftReason = "Community ban lifted";
                
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
        
        #endregion
        
        #region User Management
        
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
                .Where(u => u.DisplayName.Contains(searchTerm) || 
                           (u.User.Email != null && u.User.Email.Contains(searchTerm)))
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
                    BanExpiresAt = u.BanExpiresAt,
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
                CommunityCount = await _context.CommunityMembers.CountAsync(m => m.UserId == userId),
                RecentLogs = await GetUserModerationLogsAsync(userId, 1)
            };
            
            return detail;
        }
        
        #endregion
        
        #region Moderation Logs
        
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
                _logger.LogError(ex, "Error logging moderation action: {ActionType}", actionType);
            }
        }
        
        public async Task<List<ModerationLogViewModel>> GetModerationLogsAsync(int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            
            var logs = await _context.ModerationLogs
                .Include(l => l.Moderator)
                .Include(l => l.TargetUser)
                .Include(l => l.Community)
                .OrderByDescending(l => l.PerformedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            
            var result = new List<ModerationLogViewModel>();
            foreach (var log in logs)
            {
                var modProfile = await _context.UserProfiles.FindAsync(log.ModeratorId);
                var targetProfile = await _context.UserProfiles.FindAsync(log.TargetUserId);
                
                result.Add(new ModerationLogViewModel
                {
                    LogId = log.LogId,
                    ModeratorName = modProfile?.DisplayName ?? log.Moderator.UserName,
                    TargetUserName = targetProfile?.DisplayName ?? log.TargetUser.UserName,
                    CommunityName = log.Community?.Name,
                    ActionType = log.ActionType,
                    Reason = log.Reason,
                    PerformedAt = log.PerformedAt,
                    OldValue = log.OldValue,
                    NewValue = log.NewValue
                });
            }
            
            return result;
        }
        
        public async Task<List<ModerationLogViewModel>> GetUserModerationLogsAsync(string userId, int page = 1)
        {
            var skip = (page - 1) * 50;
            
            var logs = await _context.ModerationLogs
                .Include(l => l.Moderator)
                .Include(l => l.Community)
                .Where(l => l.TargetUserId == userId)
                .OrderByDescending(l => l.PerformedAt)
                .Skip(skip)
                .Take(50)
                .ToListAsync();
            
            var result = new List<ModerationLogViewModel>();
            foreach (var log in logs)
            {
                var modProfile = await _context.UserProfiles.FindAsync(log.ModeratorId);
                
                result.Add(new ModerationLogViewModel
                {
                    LogId = log.LogId,
                    ModeratorName = modProfile?.DisplayName ?? log.Moderator.UserName,
                    ActionType = log.ActionType,
                    Reason = log.Reason,
                    PerformedAt = log.PerformedAt,
                    CommunityName = log.Community?.Name,
                    OldValue = log.OldValue,
                    NewValue = log.NewValue
                });
            }
            
            return result;
        }
        
        #endregion
        
        #region Statistics
        
        public async Task<AdminStatsViewModel> GetAdminStatsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddDays(-30);
            
            var stats = new AdminStatsViewModel
            {
                TotalUsers = await _context.UserProfiles.CountAsync(),
                ActiveUsers = await _context.UserProfiles.CountAsync(u => u.LastActive > monthAgo),
                BannedUsers = await _context.UserProfiles.CountAsync(u => u.IsBanned),
                TotalCommunities = await _context.Communities.CountAsync(c => !c.IsDeleted),
                TotalPosts = await _context.Posts.CountAsync(),
                TotalComments = await _context.Comments.CountAsync(),
                NewUsersToday = await _context.UserProfiles.CountAsync(u => u.JoinDate >= today),
                NewUsersThisWeek = await _context.UserProfiles.CountAsync(u => u.JoinDate >= weekAgo),
                NewUsersThisMonth = await _context.UserProfiles.CountAsync(u => u.JoinDate >= monthAgo),
                ModerationActionsToday = await _context.ModerationLogs.CountAsync(l => l.PerformedAt >= today),
                ModerationActionsThisWeek = await _context.ModerationLogs.CountAsync(l => l.PerformedAt >= weekAgo),
                TopModerators = await GetTopModeratorsAsync()
            };
            
            return stats;
        }
        
        private async Task<List<TopModeratorViewModel>> GetTopModeratorsAsync()
        {
            var topMods = await _context.ModerationLogs
                .Where(l => l.PerformedAt > DateTime.UtcNow.AddDays(-30))
                .GroupBy(l => l.ModeratorId)
                .Select(g => new 
                {
                    ModeratorId = g.Key,
                    ActionCount = g.Count()
                })
                .OrderByDescending(m => m.ActionCount)
                .Take(10)
                .ToListAsync();
            
            var result = new List<TopModeratorViewModel>();
            foreach (var mod in topMods)
            {
                var profile = await _context.UserProfiles.FindAsync(mod.ModeratorId);
                result.Add(new TopModeratorViewModel
                {
                    ModeratorId = mod.ModeratorId,
                    ModeratorName = profile?.DisplayName ?? "Unknown",
                    ActionCount = mod.ActionCount
                });
            }
            
            return result;
        }
        
        #endregion
    }
}

