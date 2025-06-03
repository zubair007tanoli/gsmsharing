// ViewComponents/RightSidebarViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using System.Collections.Generic;

namespace discussionspot9.ViewComponents
{
    public class RightSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(CommunityDetailViewModel community, List<PostCardViewModel> relatedPosts)
        {
            return View((community, relatedPosts));
        }
    }
}