using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult PostManagement()
        {
            return View();
        }

        public IActionResult CommentsManagement()
        {
            return View();
        }

        public IActionResult CommunitiesManagement()
        {
            return View();
        }

        public IActionResult Categories()
        {
            return View();
        }
    }
}
