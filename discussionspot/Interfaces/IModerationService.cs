using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IModerationService
    {
        Task<bool> RemovePostAsync(int postId, string moderatorId, string reason);
        Task<bool> RemoveCommentAsync(int commentId, string moderatorId, string reason);
        Task<bool> BanUserFromCommunityAsync(string userId, int communityId, string moderatorId, string reason, int? days = null);
        Task<bool> UnbanUserFromCommunityAsync(string userId, int communityId, string moderatorId);
        Task<bool> AddModeratorAsync(string userId, int communityId, string adminId);
        Task<bool> RemoveModeratorAsync(string userId, int communityId, string adminId);
        Task<IEnumerable<UserViewModel>> GetBannedUsersAsync(int communityId);
        //Task<IEnumerable<ModerationLogViewModel>> GetModerationLogsAsync(int communityId);
        Task<bool> PinPostAsync(int postId, string moderatorId);
        Task<bool> UnpinPostAsync(int postId, string moderatorId);
        Task<bool> LockPostAsync(int postId, string moderatorId, string reason);
        Task<bool> UnlockPostAsync(int postId, string moderatorId);
    }
}
