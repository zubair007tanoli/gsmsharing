using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class CommunityInfoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(CommunityDetailViewModel community)
        {
            // Convert CommunityDetailViewModel to CommunityViewModel if needed
            var communityViewModel = new CommunityViewModel
            {
                Name = community?.Name ?? "Community",
                Slug = community?.Slug ?? "",
                Description = community?.Description ?? community?.ShortDescription ?? "Join the discussion in this community.",
                MemberCount = community?.MemberCount ?? 0,
                OnlineCount = 0, // Will be calculated from active sessions in future
                CreatedAt = community?.CreatedAt ?? DateTime.UtcNow,
                IsMember = community?.IsCurrentUserMember ?? false
            };

            return View(communityViewModel);
        }
    }
}
