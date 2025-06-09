// ViewComponents/RightSidebarViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using System.Collections.Generic;

namespace discussionspot9.Components
{
    public class RightSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}