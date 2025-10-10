using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommunityController(
            ICommunityService communityService,
            IPostService postService,
            IMemoryCache cache,
            ILogger<CommunityController> logger,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _communityService = communityService;
            _postService = postService;
            _cache = cache;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Display all communities/categories
        /// </summary>
        // Route: /communities

        [HttpGet]
        [Route("communities")]
        public async Task<IActionResult> Index(string sort = "trending", string category = null, int page = 1)
        {
            try
            {
                const int pageSize = 12;
                var userId = User.Identity.IsAuthenticated ?
                    (await _userManager.GetUserAsync(User))?.Id : null;

                // Build query
                var query = _context.Communities
                    .Include(c => c.Category)
                    .Include(c => c.Members)
                    .Where(c => !c.IsDeleted);

                // Apply category filter
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(c => c.Category != null && c.Category.Name == category);
                }

                // Apply sorting
                query = sort switch
                {
                    "newest" => query.OrderByDescending(c => c.CreatedAt),
                    "active" => query.OrderByDescending(c => c.UpdatedAt),
                    "members" => query.OrderByDescending(c => c.MemberCount),
                    _ => query.OrderByDescending(c => c.MemberCount * 0.7 + c.PostCount * 0.3) // "trending" default
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Get paginated communities
                var communities = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CommunityListItemViewModel
                    {
                        CommunityId = c.CommunityId,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.ShortDescription ?? c.Description,
                        IconUrl = c.IconUrl,
                        MemberCount = c.MemberCount,
                        PostCount = c.PostCount,
                        CategoryName = c.Category != null ? c.Category.Name : null,
                        IsCurrentUserMember = userId != null &&
                            c.Members.Any(m => m.UserId == userId),
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                // Calculate online members (mock data for now - you'd implement real tracking)
                foreach (var community in communities)
                {
                    community.OnlineMembers = Random.Shared.Next(0, Math.Max(1, community.MemberCount / 10));
                    community.Categories = community.CategoryName != null
                        ? new List<string> { community.CategoryName }
                        : new List<string>();
                }

                // Get category counts for filter
                var categoryCounts = await _context.Communities
                    .Where(c => !c.IsDeleted && c.Category != null)
                    .GroupBy(c => c.Category.Name)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Category, x => x.Count);

                var viewModel = new CommunityListViewModel
                {
                    Communities = communities,
                    CategoryCounts = categoryCounts,
                    CurrentSort = sort,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    CurrentCategory = category
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching communities");
                TempData["ErrorMessage"] = "An error occurred while loading communities.";

                // Return empty model on error
                return View(new CommunityListViewModel
                {
                    Communities = new List<CommunityListItemViewModel>(),
                    CategoryCounts = new Dictionary<string, int>(),
                    CurrentSort = sort,
                    CurrentPage = page,
                    TotalPages = 0,
                    TotalCount = 0
                });
            }
        }
        /// <summary>
        /// Display single community with its posts
        /// </summary>
        // Route: /r/{slug}
        [HttpGet]
        [Route("r/{communitySlug}")]
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
        // Route: /create-community
        [HttpGet]
        [Route("create-community")]
        [Authorize]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            var model = new CreateCommunityViewModel();
            await LoadCategories(model);
            
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        /// <summary>
        /// Create new community - POST
        /// </summary>
        // Route: /create-community (POST)
        [HttpPost]
        [Route("create-community")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateCommunityViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories(model);
                ViewData["ReturnUrl"] = returnUrl;
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
                    
                    // Redirect to return URL if provided and valid, otherwise go to community details
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Details", new { slug = result.Slug });
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create community.");
                await LoadCategories(model);
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the community.");
                await LoadCategories(model);
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }
        }

        /// <summary>
        /// Join or leave a community (AJAX)
        /// </summary>
        [HttpPost]
        [Route("api/community/togglemembership")]
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
        [Route("api/community/getmembers")]
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