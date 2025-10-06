using Microsoft.AspNetCore.Mvc;

namespace discussionspot10.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("r/{communitySlug}/posts/{postSlug}")]
        public IActionResult Details(string communitySlug, string postSlug)
        {
            ViewData["CommunitySlug"] = communitySlug;
            ViewData["PostSlug"] = postSlug;
            return View();
        }
    }
}


