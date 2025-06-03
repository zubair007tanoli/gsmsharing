// ViewComponents/CommentFormViewComponent.cs
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.ViewComponents
{
    public class CommentFormViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int postId)
        {
            ViewData["PostId"] = postId;
            return View();
        }
    }
}