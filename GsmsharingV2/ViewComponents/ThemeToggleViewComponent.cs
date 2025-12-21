using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.ViewComponents
{
    public class ThemeToggleViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

