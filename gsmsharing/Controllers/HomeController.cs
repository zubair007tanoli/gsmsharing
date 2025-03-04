using System.Diagnostics;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.Models.APIGPT;
using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryRepository _categoryService;
        private readonly AIContentGenerator _contentGenerator;
        private readonly IPostRepository postRepository;
        public HomeController(ILogger<HomeController> logger, ICategoryRepository categoryService, AIContentGenerator contentGenerator, IPostRepository postRepository)
        {
            _logger = logger;
            _categoryService = categoryService;
            _contentGenerator = contentGenerator;
            this.postRepository = postRepository;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var post = await postRepository.GetAllAsync();
            var categorySelectList = await _categoryService.CreateCategorySelectListAsync();
            ViewBag.Categories = categorySelectList;
            return View(post);
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
