using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot9.Components
{
    public class InterestingCommunitiesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InterestingCommunitiesViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            var communities = new List<InterestingCommunityViewModel>();

            try
            {
                var userId = UserClaimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                // Get communities user is NOT a member of yet
                var userCommunityIds = new List<int>();
                if (!string.IsNullOrEmpty(userId))
                {
                    userCommunityIds = await _context.CommunityMembers
                        .Where(cm => cm.UserId == userId)
                        .Select(cm => cm.CommunityId)
                        .ToListAsync();
                }

                // Get popular communities that user hasn't joined
                var query = _context.Communities
                    .Where(c => !c.IsDeleted && !userCommunityIds.Contains(c.CommunityId));

                communities = await query
                    .OrderByDescending(c => c.MemberCount)
                    .ThenByDescending(c => c.PostCount)
                    .Take(count)
                    .Select(c => new InterestingCommunityViewModel
                    {
                        CommunityId = c.CommunityId,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.Description,
                        MemberCount = c.MemberCount,
                        PostCount = c.PostCount,
                        IconUrl = c.IconUrl,
                        CategoryName = c.Category != null ? c.Category.Name : "General"
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                communities = new List<InterestingCommunityViewModel>();
            }

            return View(communities);
        }
    }
}

