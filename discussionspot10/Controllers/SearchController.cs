using Microsoft.AspNetCore.Mvc;

namespace discussionspot10.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index(string? q)
        {
            ViewData["Query"] = q ?? string.Empty;
            return View();
        }
    }
}


