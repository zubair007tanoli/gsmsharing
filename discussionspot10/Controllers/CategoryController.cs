using Microsoft.AspNetCore.Mvc;

namespace discussionspot10.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("c/{slug}")]
        public IActionResult Details(string slug)
        {
            ViewData["CategorySlug"] = slug;
            return View();
        }
    }
}


