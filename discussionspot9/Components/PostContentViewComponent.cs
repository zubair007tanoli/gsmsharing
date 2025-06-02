// ViewComponents/PostContentViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;

namespace discussionspot9.ViewComponents
{
    public class PostContentViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PostDetailViewModel post, string communitySlug)
        {
            ViewData["CommunitySlug"] = communitySlug;
            return View(post);
        }
    }
}