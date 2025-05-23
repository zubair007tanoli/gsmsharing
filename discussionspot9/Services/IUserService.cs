using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task LogoutUserAsync();
        Task<ProfileViewModel?> GetUserProfileAsync(string userId);
        Task<IdentityResult> UpdateUserProfileAsync(string userId, ProfileViewModel model);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
        Task<UserProfile?> GetUserProfileByUserIdAsync(string userId);
        Task<UserProfile?> GetUserProfileByDisplayNameAsync(string displayName);
        Task<bool> IsDisplayNameTakenAsync(string displayName, string? currentUserId = null);
        Task UpdateLastActiveAsync(string userId);
        Task<UserStatsViewModel?> GetUserStatsAsync(string userId);
        object ConfirmEmailAsync(string userId, string code);
    }
}
