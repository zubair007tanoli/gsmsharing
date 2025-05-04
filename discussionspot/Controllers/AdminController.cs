using Microsoft.AspNetCore.Mvc;

namespace discussionspot.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
