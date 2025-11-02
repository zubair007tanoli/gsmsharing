using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using discussionspot9.Helpers;

namespace discussionspot9.Components
{
    [ViewComponent(Name = "UserStatsSidebar")]
    public class UserStatsSidebarViewComponent : ViewComponent
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public UserStatsSidebarViewComponent(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
                return Content("");

            await using var context = await _contextFactory.CreateDbContextAsync();
            var userProfile = await context.UserProfiles
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (userProfile == null)
                return Content("");

            var postCount = await context.Posts
                .CountAsync(p => p.UserId == userId && p.Status == "published");

            var commentCount = await context.Comments
                .CountAsync(c => c.UserId == userId && !c.IsDeleted);

            var viewModel = new
            {
                DisplayName = userProfile.DisplayName,
                KarmaPoints = userProfile.KarmaPoints,
                KarmaLevel = KarmaHelper.GetKarmaLevel(userProfile.KarmaPoints),
                KarmaLevelName = KarmaHelper.GetKarmaLevelName(userProfile.KarmaPoints),
                KarmaProgress = KarmaHelper.GetKarmaProgressPercentage(userProfile.KarmaPoints),
                KarmaNeeded = KarmaHelper.GetKarmaNeededForNextLevel(userProfile.KarmaPoints),
                PostCount = postCount,
                CommentCount = commentCount,
                JoinDate = userProfile.JoinDate,
                IsVerified = userProfile.IsVerified,
                Initials = GetInitials(userProfile.DisplayName)
            };

            return View(viewModel);
        }

        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return "?";

            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }
    }
}

