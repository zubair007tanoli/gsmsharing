// ViewComponents/LeftSidebarViewComponent.cs
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.ViewComponents
{
    public class LeftSidebarViewComponent : ViewComponent
    {
        private readonly ICommunityService _communityService;

        public LeftSidebarViewComponent(ICommunityService communityService)
        {
            _communityService = communityService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string communitySlug)
        {
            // Get user's joined communities
            var userId = HttpContext.User.Identity.IsAuthenticated
                ? HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;

            var model = new LeftSidebarViewModel
            {
                CurrentCommunitySlug = communitySlug,
                JoinedCommunities = userId != null
                    ? await _communityService.GetUserCommunitiesAsync(userId)
                    : new List<CommunityCardViewModel>(),
                TrendingCommunities = await _communityService.GetTrendingCommunitiesAsync(5)
            };

            return View(model);
        }
    }
}