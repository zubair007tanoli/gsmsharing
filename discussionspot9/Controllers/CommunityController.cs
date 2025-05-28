using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Controllers
{
    public class CommunityController : Controller
    {
        private readonly ICommunityService _communityService;
        private readonly IPostService _postService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(
            ICommunityService communityService,
            IPostService postService,
            IMemoryCache cache,
            ILogger<CommunityController> logger)
        {
            _communityService = communityService;
            _postService = postService;
            _cache = cache;
            _logger = logger;
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
        public async Task<IActionResult> Details(string slug, string sort = "hot", int page = 1)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            try
            {
                var community = await _communityService.GetCommunityDetailsAsync(slug);
                if (community == null)
                {
                    return NotFound();
                }

                // Get posts for this community
                var posts = await _postService.GetCommunityPostsAsync(community.CommunityId, sort, page);

                ViewData["CurrentSort"] = sort;
                ViewData["CurrentPage"] = page;
                ViewData["CommunitySlug"] = slug;

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
                _logger.LogError(ex, "Error fetching community details for slug: {Slug}", slug);
                TempData["ErrorMessage"] = "An error occurred while loading the community.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create new community - GET
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            var model = new CreateCommunityViewModel();
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
    }
}