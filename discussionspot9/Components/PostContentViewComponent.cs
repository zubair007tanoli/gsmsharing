// ViewComponents/PostContentViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;

namespace discussionspot9.Components
{
    public class PostContentViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
           return View();
        }
    }
}