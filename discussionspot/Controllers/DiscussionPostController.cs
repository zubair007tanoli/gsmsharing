using discussionspot.ViewModels;
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
        [HttpGet]
        public IActionResult CreatePost()
        {
            PostCreateViewModel model = new();
            return View(model);
        }
        [HttpPost]
        public IActionResult CreatePost(PostCreateViewModel model)
        {
            return View();
        }
        public IActionResult EditPost()
        {
            return View();
        }
    }
}
