using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace discussionspot9.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserHelper(IHttpContextAccessor httpContextAccessor,
                         UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

        public string? GetCurrentUserId()
        {
            // Using claims directly is faster than UserManager.GetUserId()
            return CurrentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetCurrentUserName()
        {
            return CurrentUser?.FindFirst(ClaimTypes.Name)?.Value
                   ?? CurrentUser?.Identity?.Name;
        }

        public string? GetCurrentUserEmail()
        {
            return CurrentUser?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public bool IsAuthenticated()
        {
            return CurrentUser?.Identity?.IsAuthenticated ?? false;
        }

        public async Task<IdentityUser?> GetCurrentUserAsync()
        {
            if (!IsAuthenticated())
                return null;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public IEnumerable<string> GetCurrentUserRoles()
        {
            if (!IsAuthenticated())
                return Enumerable.Empty<string>();

            return CurrentUser?.FindAll(ClaimTypes.Role)?.Select(c => c.Value)
                   ?? Enumerable.Empty<string>();
        }

        public bool IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role) || !IsAuthenticated())
                return false;

            return CurrentUser?.IsInRole(role) ?? false;
        }
    }
}
