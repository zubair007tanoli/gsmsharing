// ViewComponents/PollWidgetViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.PollViewModels;

namespace discussionspot9.ViewComponents
{
    public class PollWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PollViewModel poll, int postId)
        {
            if (poll == null)
            {
                return Content(string.Empty);
            }

            ViewData["PostId"] = postId;
            return View(poll);
        }
    }
}