using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GsmsharingV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IPostRepository postRepository, 
            ICategoryRepository categoryRepository,
            ILogger<HomeController> logger)
        {
            _postRepository = postRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featuredPosts = await _postRepository.GetFeaturedPostsAsync();
            var recentPosts = await _postRepository.GetRecentPostsAsync(10);
            var categories = await _categoryRepository.GetTopLevelCategoriesAsync();
            
            // Get posts by category
            var postsByCategory = new Dictionary<Category, IEnumerable<Post>>();
            foreach (var category in categories.Take(6))
            {
                var posts = await _categoryRepository.GetPostsByCategoryIdAsync(category.CategoryID, 5);
                if (posts.Any())
                {
                    postsByCategory[category] = posts;
                }
            }
            
            ViewBag.FeaturedPosts = featuredPosts;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.Categories = categories;
            ViewBag.PostsByCategory = postsByCategory;
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
