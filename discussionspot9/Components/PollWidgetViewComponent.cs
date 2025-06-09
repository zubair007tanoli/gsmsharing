// ViewComponents/PollWidgetViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.PollViewModels;

namespace discussionspot9.Components
{
    public class PollWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {           
            return View();
        }
    }
}