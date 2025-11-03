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

        public IActionResult IndexPage()
        {
            return View();
        }

        public async Task<IActionResult> IndexAsync()
        {
            var publishedPosts = await postRepository.GetAllAsync();
            
            var categorySelectList = await _categoryService.CreateCategorySelectListAsync();
            ViewBag.Categories = categorySelectList;
            
            _logger.LogInformation($"Showing {publishedPosts.Count()} posts on homepage");
            
            return View(publishedPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            // Get the actual status code
            var actualStatusCode = statusCode ?? HttpContext.Response.StatusCode;
            
            // If still no status code, default to 500
            if (actualStatusCode == 200 || actualStatusCode == 0)
            {
                actualStatusCode = 500;
            }

            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = actualStatusCode
            };

            // Set user-friendly error messages based on status code
            errorViewModel.ErrorMessage = actualStatusCode switch
            {
                400 => "Bad Request - The request could not be understood by the server.",
                401 => "Unauthorized - You are not authorized to access this resource.",
                403 => "Forbidden - You don't have permission to access this resource.",
                404 => "Page Not Found - The page you are looking for doesn't exist.",
                500 => "Internal Server Error - Something went wrong on our end.",
                503 => "Service Unavailable - The service is temporarily unavailable.",
                _ => "An error occurred while processing your request."
            };

            _logger.LogError($"Error {actualStatusCode}: {errorViewModel.ErrorMessage} - RequestId: {errorViewModel.RequestId}");

            return View(errorViewModel);
        }
    }
}
