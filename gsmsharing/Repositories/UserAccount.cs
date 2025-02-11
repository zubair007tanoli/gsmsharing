using gsmsharing.Interfaces;
using gsmsharing.Models;
using Microsoft.AspNetCore.Identity;

namespace gsmsharing.Repositories
{
    public class UserAccount : IUserAccount
    {
        public Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}
