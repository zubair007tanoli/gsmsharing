using discussionspot9.Models.Domain;

namespace discussionspot9.Interfaces
{
    /// <summary>
    /// Service for managing user follow relationships
    /// </summary>
    public interface IFollowService
    {
        /// <summary>
        /// Follow a user
        /// </summary>
        Task<bool> FollowUserAsync(string followerId, string followedId);

        /// <summary>
        /// Unfollow a user
        /// </summary>
        Task<bool> UnfollowUserAsync(string followerId, string followedId);

        /// <summary>
        /// Check if one user follows another
        /// </summary>
        Task<bool> IsFollowingAsync(string followerId, string followedId);

        /// <summary>
        /// Get follower count for a user
        /// </summary>
        Task<int> GetFollowerCountAsync(string userId);

        /// <summary>
        /// Get following count for a user
        /// </summary>
        Task<int> GetFollowingCountAsync(string userId);

        /// <summary>
        /// Get list of followers for a user
        /// </summary>
        Task<List<UserFollow>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get list of users that a user is following
        /// </summary>
        Task<List<UserFollow>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get suggested users to follow based on mutual follows and interests
        /// </summary>
        Task<List<string>> GetSuggestedUsersAsync(string userId, int count = 5);

        /// <summary>
        /// Toggle follow status (follow if not following, unfollow if following)
        /// </summary>
        Task<bool> ToggleFollowAsync(string followerId, string followedId);

        /// <summary>
        /// Get mutual followers between two users
        /// </summary>
        Task<List<string>> GetMutualFollowersAsync(string userId1, string userId2);
    }
}

