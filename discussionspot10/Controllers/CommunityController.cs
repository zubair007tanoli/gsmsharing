using Microsoft.AspNetCore.Mvc;

namespace discussionspot10.Controllers
{
    public class CommunityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("r/{communitySlug}")]
        public IActionResult Details(string communitySlug)
        {
            ViewData["CommunitySlug"] = communitySlug;
            return View();
        }
    }
}


