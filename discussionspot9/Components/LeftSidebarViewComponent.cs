// ViewComponents/LeftSidebarViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using System.Collections.Generic;

namespace discussionspot9.Components
{
    [ViewComponent(Name = "LeftSideBar")]
    public class LeftSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string communitySlug)
        {
            // For now, return empty lists. You can implement the service methods later
            var model = new LeftSidebarViewModel
            {
                CurrentCommunitySlug = communitySlug,
                JoinedCommunities = new List<CommunityCardViewModel>(),
                TrendingCommunities = new List<CommunityCardViewModel>()
            };

            return View();
        }
    }
}