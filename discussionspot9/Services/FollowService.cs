using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowService> _logger;
        private readonly INotificationService? _notificationService;

        public FollowService(
            ApplicationDbContext context,
            ILogger<FollowService> logger,
            INotificationService? notificationService = null)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<bool> FollowUserAsync(string followerId, string followedId)
        {
            try
            {
                // Validate: Can't follow yourself
                if (followerId == followedId)
                {
                    _logger.LogWarning("User {FollowerId} attempted to follow themselves", followerId);
                    return false;
                }

                // Check if already following
                var existingFollow = await _context.UserFollows
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

                if (existingFollow != null)
                {
                    if (!existingFollow.IsActive)
                    {
                        // Reactivate follow
                        existingFollow.IsActive = true;
                        existingFollow.FollowedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("User {FollowerId} reactivated follow of {FollowedId}", followerId, followedId);
                        return true;
                    }

                    _logger.LogWarning("User {FollowerId} already follows {FollowedId}", followerId, followedId);
                    return false;
                }

                // Create new follow relationship
                var follow = new UserFollow
                {
                    FollowerId = followerId,
                    FollowedId = followedId,
                    FollowedAt = DateTime.UtcNow,
                    NotificationsEnabled = true,
                    IsActive = true
                };

                _context.UserFollows.Add(follow);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {FollowerId} now follows {FollowedId}", followerId, followedId);

                // Send follow notification
                if (_notificationService != null)
                {
                    try
                    {
                        var followerProfile = await _context.UserProfiles.FindAsync(followerId);
                        var followerName = followerProfile?.DisplayName ?? "Someone";
                        var followerAvatar = followerProfile?.AvatarUrl;

                        await _notificationService.CreateNotificationAsync(
                            userId: followedId,
                            title: "New Follower",
                            message: $"{followerName} started following you",
                            entityType: "user",
                            entityId: followerId,
                            type: "follow");
                    }
                    catch (Exception notifyEx)
                    {
                        _logger.LogError(notifyEx, "Error sending follow notification");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following user: {FollowerId} -> {FollowedId}", followerId, followedId);
                return false;
            }
        }

        public async Task<bool> UnfollowUserAsync(string followerId, string followedId)
        {
            try
            {
                var follow = await _context.UserFollows
                    .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId && f.IsActive);

                if (follow == null)
                {
                    _logger.LogWarning("User {FollowerId} is not following {FollowedId}", followerId, followedId);
                    return false;
                }

                // Soft delete - set IsActive to false
                follow.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {FollowerId} unfollowed {FollowedId}", followerId, followedId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing user: {FollowerId} -> {FollowedId}", followerId, followedId);
                return false;
            }
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followedId)
        {
            try
            {
                return await _context.UserFollows
                    .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId && f.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if {FollowerId} follows {FollowedId}", followerId, followedId);
                return false;
            }
        }

        public async Task<int> GetFollowerCountAsync(string userId)
        {
            try
            {
                return await _context.UserFollows
                    .CountAsync(f => f.FollowedId == userId && f.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follower count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            try
            {
                return await _context.UserFollows
                    .CountAsync(f => f.FollowerId == userId && f.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<List<UserFollow>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                return await _context.UserFollows
                    .Where(f => f.FollowedId == userId && f.IsActive)
                    .Include(f => f.Follower)
                    .OrderByDescending(f => f.FollowedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting followers for user {UserId}", userId);
                return new List<UserFollow>();
            }
        }

        public async Task<List<UserFollow>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                return await _context.UserFollows
                    .Where(f => f.FollowerId == userId && f.IsActive)
                    .Include(f => f.Followed)
                    .OrderByDescending(f => f.FollowedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following for user {UserId}", userId);
                return new List<UserFollow>();
            }
        }

        public async Task<List<string>> GetSuggestedUsersAsync(string userId, int count = 5)
        {
            try
            {
                // Get users that the current user's followees are following
                // but the current user is not following yet
                var following = await _context.UserFollows
                    .Where(f => f.FollowerId == userId && f.IsActive)
                    .Select(f => f.FollowedId)
                    .ToListAsync();

                var suggestions = await _context.UserFollows
                    .Where(f => following.Contains(f.FollowerId) && 
                                f.IsActive && 
                                f.FollowedId != userId &&
                                !following.Contains(f.FollowedId))
                    .GroupBy(f => f.FollowedId)
                    .OrderByDescending(g => g.Count()) // Most popular among people you follow
                    .Select(g => g.Key)
                    .Take(count)
                    .ToListAsync();

                // If not enough suggestions, add some active users
                if (suggestions.Count < count)
                {
                    var additionalUsers = await _context.UserProfiles
                        .Where(up => up.UserId != userId && 
                                    !following.Contains(up.UserId) && 
                                    !suggestions.Contains(up.UserId))
                        .OrderByDescending(up => up.KarmaPoints)
                        .Take(count - suggestions.Count)
                        .Select(up => up.UserId)
                        .ToListAsync();

                    suggestions.AddRange(additionalUsers);
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggested users for {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> ToggleFollowAsync(string followerId, string followedId)
        {
            try
            {
                var isFollowing = await IsFollowingAsync(followerId, followedId);

                if (isFollowing)
                {
                    return await UnfollowUserAsync(followerId, followedId);
                }
                else
                {
                    return await FollowUserAsync(followerId, followedId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling follow: {FollowerId} -> {FollowedId}", followerId, followedId);
                return false;
            }
        }

        public async Task<List<string>> GetMutualFollowersAsync(string userId1, string userId2)
        {
            try
            {
                var user1Followers = await _context.UserFollows
                    .Where(f => f.FollowedId == userId1 && f.IsActive)
                    .Select(f => f.FollowerId)
                    .ToListAsync();

                var user2Followers = await _context.UserFollows
                    .Where(f => f.FollowedId == userId2 && f.IsActive)
                    .Select(f => f.FollowerId)
                    .ToListAsync();

                return user1Followers.Intersect(user2Followers).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mutual followers for {UserId1} and {UserId2}", userId1, userId2);
                return new List<string>();
            }
        }
    }
}

