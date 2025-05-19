using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IUserService
    {
        Task<UserViewModel> GetUserByIdAsync(string userId);
        Task<UserViewModel> GetUserByUsernameAsync(string username);
        Task<UserViewModel> GetUserWithProfileAsync(string userId);
        Task<IEnumerable<UserViewModel>> GetTopUsersAsync(int count);
        Task<IEnumerable<PostViewModel>> GetUserPostsAsync(string userId);
        Task<IEnumerable<CommentViewModel>> GetUserCommentsAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UserViewModel model);
        Task<string?> UploadUserAvatarAsync(string userId, IFormFile file);
        Task<string?> UploadUserBannerAsync(string userId, IFormFile file);
        Task<bool> FollowUserAsync(string followerId, string followingId);
        Task<bool> UnfollowUserAsync(string followerId, string followingId);
        Task<IEnumerable<UserViewModel>> GetUserFollowersAsync(string userId);
        Task<IEnumerable<UserViewModel>> GetUserFollowingAsync(string userId);
        Task<int> UpdateKarmaPointsAsync(string userId, int points);
    }
}
