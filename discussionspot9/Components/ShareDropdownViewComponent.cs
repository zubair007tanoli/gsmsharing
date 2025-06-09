// ViewComponents/ShareDropdownViewComponent.cs
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class ShareDropdownViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int postId, string title, string url)
        {
            ViewData["PostId"] = postId;
            ViewData["Title"] = title;
            ViewData["Url"] = url;
            ViewData["FullUrl"] = $"{Request.Scheme}://{Request.Host}{url}";

            return View();
        }
    }
}