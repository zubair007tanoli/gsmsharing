using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile> GetByIdAsync(int id);
        Task<UserProfile> GetByUserIdAsync(string userId);
        Task<IEnumerable<UserProfile>> GetAllAsync();
        Task<UserProfile> CreateAsync(UserProfile userProfile);
        Task<UserProfile> UpdateAsync(UserProfile userProfile);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<UserProfile>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<UserProfile>> GetByLocationAsync(string location);
    }
}
