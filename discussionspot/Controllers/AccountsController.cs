using Microsoft.AspNetCore.Mvc;

namespace discussionspot.Controllers
{
    public class AccountsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
