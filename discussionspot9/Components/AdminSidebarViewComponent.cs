using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class AdminSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

