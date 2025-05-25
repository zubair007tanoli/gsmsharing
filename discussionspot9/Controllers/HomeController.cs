using discussionspot9.Interfaces;
using discussionspot9.Models;
using discussionspot9.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace discussionspot9.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _homeService.GetHomePageDataAsync();
            return View(model);
        }

        public async Task<IActionResult> Popular(string? timeframe = null)
        {
            var model = await _homeService.GetHomePageDataAsync();
            ViewData["Timeframe"] = timeframe;
            return View("Index", model);
        }

        public async Task<IActionResult> All(string? sort = null)
        {
            var model = await _homeService.GetHomePageDataAsync();
            ViewData["Sort"] = sort;
            return View("Index", model);
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