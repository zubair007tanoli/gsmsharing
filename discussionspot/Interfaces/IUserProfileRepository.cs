using discussionspot.Models.Domain;

namespace discussionspot.Interfaces
{
    public interface IUserProfileRepository : IRepository<UserProfile>
    {
        Task<UserProfile?> GetProfileByUserIdAsync(string userId);
        Task<IEnumerable<UserProfile>> GetTopUsersByKarmaAsync(int count);
        Task UpdateKarmaPointsAsync(string userId, int points);
        Task<IEnumerable<UserProfile>> GetRecentlyActiveUsersAsync(int count);
    }
}
