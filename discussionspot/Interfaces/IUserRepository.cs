using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUsers>
    {
        Task<ApplicationUsers?> GetUserWithProfileAsync(string userId);
        Task<IEnumerable<ApplicationUsers>> GetUsersByRoleAsync(string role);
        Task<IEnumerable<ApplicationUsers>> SearchUsersAsync(string searchTerm);
        Task<bool> IsUserInRoleAsync(string userId, string role);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<bool> AddUserToRoleAsync(string userId, string role);
        Task<bool> RemoveUserFromRoleAsync(string userId, string role);
    }
}
