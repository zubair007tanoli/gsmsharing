using Microsoft.AspNetCore.Mvc;

namespace discussionspot.Controllers
{
    public class DiscussionPostController : Controller
    {

        public IActionResult GetAllPosts()
        {
            return View();
        }

        public IActionResult Post()
        {
            return View();
        }

        public IActionResult CreatePost()
        {
            return View();
        }
        public IActionResult EditPost()
        {
            return View();
        }
    }
}
