using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Helpers
{
    public interface IUserHelper
    {
        Task<IdentityUser?> GetCurrentUserAsync();
        string? GetCurrentUserEmail();
        string? GetCurrentUserId();
        string? GetCurrentUserName();
        IEnumerable<string> GetCurrentUserRoles();
        bool IsAuthenticated();
        bool IsInRole(string role);
    }
}