using Microsoft.AspNetCore.Mvc;

namespace discussionspot10.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}


