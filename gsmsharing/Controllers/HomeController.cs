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
        public HomeController(ILogger<HomeController> logger, ICategoryRepository categoryService, AIContentGenerator contentGenerator)
        {
            _logger = logger;
            _categoryService = categoryService;
            _contentGenerator = contentGenerator;
        }

        public async Task<IActionResult> IndexAsync()
        {
           
            var categorySelectList = await _categoryService.CreateCategorySelectListAsync();
            ViewBag.Categories = categorySelectList;
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
