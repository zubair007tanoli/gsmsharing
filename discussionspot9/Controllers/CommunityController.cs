using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Controllers
{
    public class CommunityController : Controller
    {
        private readonly ICommunityService _communityService;
        private readonly IPostService _postService;
        private readonly ApplicationDbContext _context; // Assuming you have a DbContext for database access
        private readonly IMemoryCache _cache;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(
            ICommunityService communityService,
            IPostService postService,
            IMemoryCache cache,
            ILogger<CommunityController> logger,
            ApplicationDbContext context)
        {
            _communityService = communityService;
            _postService = postService;
            _cache = cache;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Display all communities/categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string sort = "popular", int page = 1)
        {
            try
            {
                var model = await _communityService.GetAllCommunitiesAsync(sort, page);

                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = page;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching communities");
                TempData["ErrorMessage"] = "An error occurred while loading communities.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Display single community with its posts
        /// </summary>
        [HttpGet]
        [Route("r/{communitySlug}/Community")]
        public async Task<IActionResult> Details(string communitySlug, string sort = "hot", int page = 1)
        {
       
            if (string.IsNullOrEmpty(communitySlug))
            {
                return NotFound();
            }

            try
            {
                var community = await _communityService.GetCommunityDetailsAsync(communitySlug);
                if (community == null)
                {
                    return NotFound();
                }

                // Get posts for this community
                var posts = await _postService.GetCommunityPostsAsync(community.CommunityId, sort, page);

                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = page;
                ViewData["CommunitySlug"] = communitySlug;

                var viewModel = new CommunityDetailPageViewModel
                {
                    Community = community,
                    Posts = posts,
                    CurrentSort = sort,
                    CurrentPage = page
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching community details for slug: {Slug}", communitySlug);
                TempData["ErrorMessage"] = "An error occurred while loading the community.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create new community - GET
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult Create(string returnUrl)
        {
            var model = new CreateCommunityViewModel();
            LoadCategories(model).GetAwaiter().GetResult(); // Load categories synchronously for simplicity
            return View(model);
        }

        /// <summary>
        /// Create new community - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateCommunityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                model.CreatorId = userId;
                var result = await _communityService.CreateCommunityAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Community created successfully!";
                    return RedirectToAction("Details", new { slug = result.Slug });
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create community.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the community.");
                return View(model);
            }
        }

        /// <summary>
        /// Join or leave a community (AJAX)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleMembership(int communityId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _communityService.ToggleMembershipAsync(communityId, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling community membership");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get community members (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMembers(int communityId, int page = 1)
        {
            try
            {
                var members = await _communityService.GetCommunityMembersAsync(communityId, page);
                return PartialView("_MembersList", members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching community members");
                return StatusCode(500, "An error occurred while loading members");
            }
        }

        /// <summary>
        /// Loads active categories into view model
        /// </summary>
        private async Task LoadCategories(CreateCommunityViewModel model)
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new CategoryDropdownItem
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name
                    })
                    .ToListAsync();

                model.AvailableCategories = categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories");
                model.AvailableCategories = new List<CategoryDropdownItem>();
                throw;
            }
        }

        /// <summary>
        /// Validates category selection against database
        /// </summary>
        private async Task<bool> ValidateCategory(int categoryId)
        {
            if (categoryId <= 0) return false;

            return await _context.Categories
                .AnyAsync(c => c.CategoryId == categoryId && c.IsActive);
        }
    }
}