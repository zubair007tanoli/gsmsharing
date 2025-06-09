using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class CommunityInfoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string communitySlug)
        {
            var community = new CommunityViewModel
            {
                Name = "Technology",
                Slug = communitySlug,
                Description = "A community dedicated to the discussion of technology news, developments, and innovations.",
                MemberCount = 2400000,              
                CreatedAt = new DateTime(2012, 1, 1)
            };

            return View(community);
        }
    }
}
