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
        private readonly EnhancedHomeService _enhancedHomeService;

        public HomeController(
            ILogger<HomeController> logger, 
            IHomeService homeService,
            EnhancedHomeService enhancedHomeService)
        {
            _logger = logger;
            _homeService = homeService;
            _enhancedHomeService = enhancedHomeService;
        }

        [ResponseCache(Duration = 300, VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any)] // 5 minutes cache
        public async Task<IActionResult> Index()
        {
            // Use new enhanced homepage
            var model = await _enhancedHomeService.GetEnhancedHomePageDataAsync();
            return View("IndexNew", model); // Using new view
        }
        
        // Keep old version accessible
        public async Task<IActionResult> IndexOld()
        {
            var model = await _homeService.GetHomePageDataAsync();
            return View("Index", model);
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

        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // 24 hours cache
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Privacy Policy - DiscussionSpot";
            ViewData["Description"] = "Read our Privacy Policy to understand how we collect, use, and protect your personal information on DiscussionSpot.";
            return View();
        }

        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // 24 hours cache
        public IActionResult Terms()
        {
            ViewData["Title"] = "Terms of Service - DiscussionSpot";
            ViewData["Description"] = "Review our Terms of Service to understand the rules and regulations for using DiscussionSpot.";
            return View();
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // 1 hour cache
        public IActionResult About()
        {
            ViewData["Title"] = "About Us - DiscussionSpot";
            ViewData["Description"] = "Learn about DiscussionSpot, our mission, values, and the team behind this thriving community platform.";
            return View();
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // 1 hour cache
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us - DiscussionSpot";
            ViewData["Description"] = "Get in touch with the DiscussionSpot team. We'd love to hear from you!";
            return View();
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // 1 hour cache
        public IActionResult FAQ()
        {
            ViewData["Title"] = "Frequently Asked Questions - DiscussionSpot";
            ViewData["Description"] = "Find answers to commonly asked questions about DiscussionSpot features, policies, and usage.";
            return View();
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // 1 hour cache
        public IActionResult Advertise()
        {
            ViewData["Title"] = "Advertise With Us - DiscussionSpot";
            ViewData["Description"] = "Reach millions of engaged users on DiscussionSpot. Learn about our advertising opportunities.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}