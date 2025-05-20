using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiscussionSpot9.Services
{

    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public UserService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            // Check if display name is taken
            if (await IsDisplayNameTakenAsync(model.DisplayName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateDisplayName",
                    Description = "This display name is already taken."
                });
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true // You might want to implement email confirmation
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add user to User role
                await _userManager.AddToRoleAsync(user, "User");

                // Create UserProfile
                var userProfile = new UserProfile
                {
                    UserId = user.Id,
                    DisplayName = model.DisplayName,
                    Bio = model.Bio,
                    Location = model.Location,
                    Website = model.Website,
                    JoinDate = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    KarmaPoints = 0,
                    IsVerified = false
                };

                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return result;
        }

        public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Update last active
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await UpdateLastActiveAsync(user.Id);
                }
            }

            return result;
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ProfileViewModel?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null) return null;

            return new ProfileViewModel
            {
                UserId = userId,
                Email = user.Email!,
                DisplayName = userProfile.DisplayName,
                Bio = userProfile.Bio,
                Location = userProfile.Location,
                Website = userProfile.Website,
                AvatarUrl = userProfile.AvatarUrl,
                BannerUrl = userProfile.BannerUrl,
                KarmaPoints = userProfile.KarmaPoints,
                IsVerified = userProfile.IsVerified,
                JoinDate = userProfile.JoinDate,
                LastActive = userProfile.LastActive
            };
        }

        public async Task<IdentityResult> UpdateUserProfileAsync(string userId, ProfileViewModel model)
        {
            // Check if display name is taken by another user
            if (await IsDisplayNameTakenAsync(model.DisplayName, userId))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateDisplayName",
                    Description = "This display name is already taken."
                });
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "User profile not found."
                });
            }

            userProfile.DisplayName = model.DisplayName;
            userProfile.Bio = model.Bio;
            userProfile.Location = model.Location;
            userProfile.Website = model.Website;
            userProfile.AvatarUrl = model.AvatarUrl;
            userProfile.BannerUrl = model.BannerUrl;

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "User not found."
                });
            }

            return await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        }

        public async Task<UserProfile?> GetUserProfileByUserIdAsync(string userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task<UserProfile?> GetUserProfileByDisplayNameAsync(string displayName)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.DisplayName == displayName);
        }

        public async Task<bool> IsDisplayNameTakenAsync(string displayName, string? currentUserId = null)
        {
            var query = _context.UserProfiles.Where(up => up.DisplayName == displayName);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(up => up.UserId != currentUserId);
            }

            return await query.AnyAsync();
        }

        public async Task UpdateLastActiveAsync(string userId)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile != null)
            {
                userProfile.LastActive = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserStatsViewModel?> GetUserStatsAsync(string userId)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null) return null;

            var postCount = await _context.Posts
                .CountAsync(p => p.UserId == userId && p.Status == "published");

            var commentCount = await _context.Comments
                .CountAsync(c => c.UserId == userId && !c.IsDeleted);

            return new UserStatsViewModel
            {
                DisplayName = userProfile.DisplayName,
                KarmaPoints = userProfile.KarmaPoints,
                PostCount = postCount,
                CommentCount = commentCount,
                JoinDate = userProfile.JoinDate,
                IsVerified = userProfile.IsVerified,
                AvatarUrl = userProfile.AvatarUrl
            };
        }

        //public async Task<UserProfile?> GetUserProfileByDisplayNameAsync(string displayName)
        //{
        //    return await _context.UserProfiles
        //        .FirstOrDefaultAsync(up => up.DisplayName == displayName);
        //}
    }
}