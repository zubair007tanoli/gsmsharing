using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.AdminViewModels;

namespace discussionspot9.Interfaces
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
        Task<List<string>> GetAllAdminUserIdsAsync();
        
        // Moderation Logs
        Task LogModerationActionAsync(string moderatorId, string targetUserId, string actionType, string reason, string? entityId = null, int? communityId = null);
        Task<List<ModerationLogViewModel>> GetModerationLogsAsync(int page = 1, int pageSize = 50);
        Task<List<ModerationLogViewModel>> GetUserModerationLogsAsync(string userId, int page = 1);
        
        // Statistics
        Task<AdminStatsViewModel> GetAdminStatsAsync();
    }
}

