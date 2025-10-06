using Microsoft.AspNetCore.Mvc;

namespace discussionspot10.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index(string? username)
        {
            ViewData["Username"] = username ?? "me";
            return View();
        }
    }
}


