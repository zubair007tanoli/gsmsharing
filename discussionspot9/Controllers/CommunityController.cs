using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Models.ViewModels;
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

        private readonly IAdminService _adminService;
        
        public CommunityController(
            ICommunityService communityService,
            IPostService postService,
            IMemoryCache cache,
            ILogger<CommunityController> logger,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IAdminService adminService)
        {
            _communityService = communityService;
            _postService = postService;
            _cache = cache;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _adminService = adminService;
        }

        /// <summary>
        /// Display all communities/categories
        /// </summary>
        // Route: /communities

        [HttpGet]
        [Route("communities")]
        public async Task<IActionResult> Index(string sort = "trending", string? category = null, int page = 1)
        {
            try
            {
                const int pageSize = 12;
                var userId = User.Identity.IsAuthenticated ?
                    (await _userManager.GetUserAsync(User))?.Id : string.Empty;

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
                        CategoryName = c.Category != null ? c.Category.Name : "",
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
                    CurrentCategory = category ?? string.Empty,
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

                // Get current user ID if authenticated
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Check if user is a member
                    community.IsCurrentUserMember = await _context.CommunityMembers
                        .AnyAsync(cm => cm.CommunityId == community.CommunityId && cm.UserId == userId);
                    
                    // Check if user is moderator or creator
                    var memberRole = await _context.CommunityMembers
                        .Where(cm => cm.CommunityId == community.CommunityId && cm.UserId == userId)
                        .Select(cm => cm.Role)
                        .FirstOrDefaultAsync();
                    
                    community.IsCurrentUserModerator = memberRole == "moderator" || 
                                                       memberRole == "admin" || 
                                                       community.CreatorId == userId;
                    community.CurrentUserRole = memberRole;
                    community.CurrentUserId = userId;
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
            
            // Get popular categories with community counts
            var popularCategories = await _context.Communities
                .Where(c => !c.IsDeleted && c.Category != null)
                .GroupBy(c => new { c.Category.CategoryId, c.Category.Name })
                .Select(g => new 
                { 
                    CategoryName = g.Key.Name,
                    CommunityCount = g.Count()
                })
                .OrderByDescending(x => x.CommunityCount)
                .Take(4)
                .ToListAsync();
            
            ViewData["PopularCategories"] = popularCategories;
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }
        
        /// <summary>
        /// Community Settings - Admin/Moderator only
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("r/{slug}/settings")]
        public async Task<IActionResult> Settings(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var community = await _communityService.GetCommunityDetailsAsync(slug);
            if (community == null)
            {
                return NotFound();
            }
            
            // Check if user is admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(community.CommunityId, userId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(community.CommunityId, userId);
            
            if (!isAdmin && !isModerator)
            {
                TempData["ErrorMessage"] = "You don't have permission to access community settings";
                return RedirectToAction("Details", new { communitySlug = slug });
            }
            
            return View(community);
        }

        /// <summary>
        /// Community Members Management - Admin/Moderator only
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("r/{slug}/members")]
        public async Task<IActionResult> Members(string slug, int page = 1, string search = "", string role = "all", string sort = "joined", string order = "desc")
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var community = await _communityService.GetCommunityDetailsAsync(slug);
            if (community == null)
            {
                return NotFound();
            }
            
            // Check if user is admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(community.CommunityId, userId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(community.CommunityId, userId);
            
            if (!isAdmin && !isModerator)
            {
                TempData["ErrorMessage"] = "You don't have permission to access community members";
                return RedirectToAction("Details", new { communitySlug = slug });
            }
            
            var model = new CommunityMembersViewModel
            {
                CommunityId = community.CommunityId,
                CommunityName = community.Title,
                CommunitySlug = community.Slug,
                CurrentPage = page,
                SearchTerm = search,
                RoleFilter = role,
                SortBy = sort,
                SortOrder = order,
                CanManageMembers = isAdmin || isModerator,
                CanChangeRoles = isAdmin,
                CanBanMembers = isAdmin || isModerator
            };
            
            // Load members data (simplified for now)
            model.Members = await LoadCommunityMembers(community.CommunityId, page, search, role, sort, order);
            model.TotalMembers = await GetTotalMembersCount(community.CommunityId, search, role);
            // TotalPages is computed property, no need to set it
            
            return View(model);
        }

        /// <summary>
        /// Community Rules Management - Admin/Moderator only
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("r/{slug}/rules")]
        public async Task<IActionResult> Rules(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var community = await _communityService.GetCommunityDetailsAsync(slug);
            if (community == null)
            {
                return NotFound();
            }
            
            // Check if user is admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(community.CommunityId, userId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(community.CommunityId, userId);
            
            if (!isAdmin && !isModerator)
            {
                TempData["ErrorMessage"] = "You don't have permission to access community rules";
                return RedirectToAction("Details", new { communitySlug = slug });
            }
            
            var model = new CommunityRulesViewModel
            {
                CommunityId = community.CommunityId,
                CommunityName = community.Title,
                CommunitySlug = community.Slug,
                CanEditRules = isAdmin || isModerator,
                CanDeleteRules = isAdmin,
                CanReorderRules = isAdmin || isModerator
            };
            
            // Load rules and templates
            model.Rules = await LoadCommunityRules(community.CommunityId);
            model.RuleTemplates = await LoadRuleTemplates();
            
            return View(model);
        }

        /// <summary>
        /// Community Analytics - Admin/Moderator only
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("r/{slug}/analytics")]
        public async Task<IActionResult> Analytics(string slug, string range = "30d")
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var community = await _communityService.GetCommunityDetailsAsync(slug);
            if (community == null)
            {
                return NotFound();
            }
            
            // Check if user is admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(community.CommunityId, userId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(community.CommunityId, userId);
            
            if (!isAdmin && !isModerator)
            {
                TempData["ErrorMessage"] = "You don't have permission to access community analytics";
                return RedirectToAction("Details", new { communitySlug = slug });
            }
            
            var model = new CommunityAnalyticsViewModel
            {
                CommunityId = community.CommunityId,
                CommunityName = community.Title,
                CommunitySlug = community.Slug,
                DateRange = range,
                CanExportData = isAdmin || isModerator
            };
            
            // Set date range
            switch (range)
            {
                case "7d":
                    model.StartDate = DateTime.Now.AddDays(-7);
                    break;
                case "30d":
                    model.StartDate = DateTime.Now.AddDays(-30);
                    break;
                case "90d":
                    model.StartDate = DateTime.Now.AddDays(-90);
                    break;
                case "1y":
                    model.StartDate = DateTime.Now.AddYears(-1);
                    break;
                default:
                    model.StartDate = DateTime.Now.AddDays(-30);
                    break;
            }
            model.EndDate = DateTime.Now;
            
            // Load analytics data
            await LoadAnalyticsData(model);
            
            return View(model);
        }

        /// <summary>
        /// Content Filters Management - Admin/Moderator only
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("r/{slug}/filters")]
        public async Task<IActionResult> Filters(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var community = await _communityService.GetCommunityDetailsAsync(slug);
            if (community == null)
            {
                return NotFound();
            }
            
            // Check if user is admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(community.CommunityId, userId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(community.CommunityId, userId);
            
            if (!isAdmin && !isModerator)
            {
                TempData["ErrorMessage"] = "You don't have permission to access content filters";
                return RedirectToAction("Details", new { communitySlug = slug });
            }
            
            var model = new ContentFiltersViewModel
            {
                CommunityId = community.CommunityId,
                CommunityName = community.Title,
                CommunitySlug = community.Slug,
                CanEditFilters = isAdmin || isModerator,
                CanViewLogs = isAdmin || isModerator
            };
            
            // Load filter data
            await LoadFilterData(model);
            
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
        public async Task<IActionResult> ToggleMembership([FromBody] ToggleMembershipRequest request)
        {
            try
            {
                _logger.LogInformation("ToggleMembership called for community {CommunityId}", request?.CommunityId);
                
                if (request == null || request.CommunityId <= 0)
                {
                    _logger.LogWarning("Invalid request: CommunityId is {CommunityId}", request?.CommunityId);
                    return Json(new { success = false, message = "Invalid community ID" });
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated");
                    return Json(new { success = false, message = "User not authenticated" });
                }

                _logger.LogInformation("User {UserId} toggling membership for community {CommunityId}", userId, request.CommunityId);
                var result = await _communityService.ToggleMembershipAsync(request.CommunityId, userId);
                
                _logger.LogInformation("Toggle result: {Success}, IsMember: {IsMember}", result.Success, result.Data);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling community membership");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        
        // Request model for toggle membership
        public class ToggleMembershipRequest
        {
            public int CommunityId { get; set; }
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
                        Name = c.Name,
                        Icon = GetCategoryIcon(c.Name), // Auto-assign icons based on name
                        Description = c.Description
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
        /// Get appropriate FontAwesome icon for category name
        /// </summary>
        private static string GetCategoryIcon(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                var name when name.Contains("tech") || name.Contains("programming") => "fa-microchip",
                var name when name.Contains("gaming") || name.Contains("game") => "fa-gamepad",
                var name when name.Contains("science") => "fa-flask",
                var name when name.Contains("sport") => "fa-football-ball",
                var name when name.Contains("entertain") || name.Contains("movie") => "fa-film",
                var name when name.Contains("business") => "fa-briefcase",
                var name when name.Contains("health") || name.Contains("fitness") => "fa-heartbeat",
                var name when name.Contains("education") || name.Contains("learning") => "fa-graduation-cap",
                var name when name.Contains("music") => "fa-music",
                var name when name.Contains("art") || name.Contains("design") => "fa-palette",
                var name when name.Contains("food") || name.Contains("cooking") => "fa-utensils",
                var name when name.Contains("travel") => "fa-plane",
                var name when name.Contains("news") => "fa-newspaper",
                var name when name.Contains("photo") => "fa-camera",
                var name when name.Contains("book") || name.Contains("read") => "fa-book",
                var name when name.Contains("car") || name.Contains("auto") => "fa-car",
                var name when name.Contains("fashion") => "fa-tshirt",
                var name when name.Contains("home") || name.Contains("garden") => "fa-home",
                var name when name.Contains("pet") || name.Contains("animal") => "fa-paw",
                var name when name.Contains("finance") || name.Contains("money") => "fa-dollar-sign",
                _ => "fa-folder" // Default icon
            };
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
        
        // ===============================================
        // COMMUNITY MODERATION (NEW)
        // ===============================================
        
        /// <summary>
        /// Ban user from community (Community Admin/Moderator only)
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/community/ban-member")]
        public async Task<IActionResult> BanMemberFromCommunity(int communityId, string userId, string reason, int? banDurationDays)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            // Check if current user is community admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(communityId, currentUserId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(communityId, currentUserId);
            
            if (!isAdmin && !isModerator)
            {
                return Json(new { success = false, message = "Insufficient permissions - Admin or Moderator role required" });
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Ban reason is required" });
            }
            
            DateTime? expiresAt = null;
            if (banDurationDays.HasValue && banDurationDays.Value > 0)
            {
                expiresAt = DateTime.UtcNow.AddDays(banDurationDays.Value);
            }
            
            var result = await _adminService.BanUserFromCommunityAsync(userId, communityId, currentUserId, reason, expiresAt);
            
            if (result)
            {
                _logger.LogInformation("User {UserId} banned from community {CommunityId} by {ModeratorId}", userId, communityId, currentUserId);
                return Json(new { success = true, message = "User banned from community" });
            }
            
            return Json(new { success = false, message = "Failed to ban user from community" });
        }
        
        /// <summary>
        /// Unban user from community
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/community/unban-member")]
        public async Task<IActionResult> UnbanMemberFromCommunity(int communityId, string userId)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            // Check if current user is community admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(communityId, currentUserId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(communityId, currentUserId);
            
            if (!isAdmin && !isModerator)
            {
                return Json(new { success = false, message = "Insufficient permissions - Admin or Moderator role required" });
            }
            
            var result = await _adminService.UnbanUserFromCommunityAsync(userId, communityId, currentUserId);
            
            if (result)
            {
                _logger.LogInformation("User {UserId} unbanned from community {CommunityId} by {ModeratorId}", userId, communityId, currentUserId);
                return Json(new { success = true, message = "User unbanned from community" });
            }
            
            return Json(new { success = false, message = "Failed to unban user from community" });
        }
        
        /// <summary>
        /// Change community member role (Community Admin only)
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("api/community/change-member-role")]
        public async Task<IActionResult> ChangeMemberRole(int communityId, string userId, string newRole)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            // Check if current user is community admin
            var isAdmin = await _communityService.IsCommunityAdminAsync(communityId, currentUserId);
            
            if (!isAdmin)
            {
                return Json(new { success = false, message = "Only community admins can change member roles" });
            }
            
            // Validate role
            if (newRole != "member" && newRole != "moderator" && newRole != "admin")
            {
                return Json(new { success = false, message = "Invalid role. Must be 'member', 'moderator', or 'admin'" });
            }
            
            var result = await _communityService.PromoteDemoteCommunityMemberAsync(communityId, userId, newRole, currentUserId);
            
            if (result.Success)
            {
                _logger.LogInformation("User {UserId} role changed to {NewRole} in community {CommunityId} by {AdminId}", 
                    userId, newRole, communityId, currentUserId);
                    
                // Log to moderation system
                await _adminService.LogModerationActionAsync(currentUserId, userId, "role_change_community", 
                    $"Role changed to {newRole}", null, communityId);
            }
            
            return Json(result);
        }
        
        /// <summary>
        /// Get community members for moderation (Admin/Moderator only)
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("api/community/members-management/{communityId}")]
        public async Task<IActionResult> GetMembersForManagement(int communityId, int page = 1)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }
            
            // Check if current user is community admin/moderator
            var isAdmin = await _communityService.IsCommunityAdminAsync(communityId, currentUserId);
            var isModerator = await _communityService.IsCommunityModeratorAsync(communityId, currentUserId);
            
            if (!isAdmin && !isModerator)
            {
                return Json(new { success = false, message = "Insufficient permissions" });
            }
            
            var members = await _communityService.GetCommunityMembersAsync(communityId, page);
            return Json(new { success = true, members });
        }
        
        /// <summary>
        /// Get user's communities (for sidebar)
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("api/community/user-communities")]
        public async Task<IActionResult> GetUserCommunities()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Not authenticated" });
                }
                
                var communities = await _context.CommunityMembers
                    .Where(m => m.UserId == userId)
                    .Include(m => m.Community)
                    .OrderByDescending(m => m.JoinedAt)
                    .Take(10)
                    .Select(m => new
                    {
                        slug = m.Community.Slug,
                        name = m.Community.Name,
                        iconUrl = m.Community.IconUrl,
                        memberCount = m.Community.MemberCount,
                        isAdmin = m.Role == "admin",
                        isModerator = m.Role == "moderator"
                    })
                    .ToListAsync();
                
                return Json(new { success = true, communities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user communities");
                return Json(new { success = false, message = "Error loading communities" });
            }
        }
        
        /// <summary>
        /// Get suggested/popular communities (for sidebar)
        /// </summary>
        [HttpGet]
        [Route("api/community/suggested")]
        public async Task<IActionResult> GetSuggestedCommunities(int limit = 5, int? exclude = null)
        {
            try
            {
                var query = _context.Communities
                    .Where(c => !c.IsDeleted);
                
                if (exclude.HasValue)
                {
                    query = query.Where(c => c.CommunityId != exclude.Value);
                }
                
                var communities = await query
                    .OrderByDescending(c => c.MemberCount)
                    .Take(limit)
                    .Select(c => new
                    {
                        slug = c.Slug,
                        name = c.Name,
                        iconUrl = c.IconUrl,
                        memberCount = c.MemberCount,
                        postCount = c.PostCount
                    })
                    .ToListAsync();
                
                return Json(new { success = true, communities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suggested communities");
                return Json(new { success = false, message = "Error loading suggestions" });
            }
        }
        
        /// <summary>
        /// Get popular communities (fallback)
        /// </summary>
        [HttpGet]
        [Route("api/community/popular")]
        public async Task<IActionResult> GetPopularCommunities(int limit = 5)
        {
            try
            {
                var communities = await _context.Communities
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.MemberCount)
                    .ThenByDescending(c => c.PostCount)
                    .Take(limit)
                    .Select(c => new
                    {
                        slug = c.Slug,
                        name = c.Name,
                        iconUrl = c.IconUrl,
                        memberCount = c.MemberCount
                    })
                    .ToListAsync();
                
                return Json(new { communities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching popular communities");
                return Json(new { communities = new List<object>() });
            }
        }

        // ===============================================
        // HELPER METHODS FOR NEW COMMUNITY MANAGEMENT PAGES
        // ===============================================

        /// <summary>
        /// Load community members with pagination and filtering
        /// </summary>
        private async Task<List<discussionspot9.Models.ViewModels.MemberViewModel>> LoadCommunityMembers(int communityId, int page, string search, string role, string sort, string order)
        {
            var query = _context.CommunityMembers
                .Where(cm => cm.CommunityId == communityId)
                .Include(cm => cm.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(cm => cm.User.UserName.Contains(search) || 
                                        cm.User.Email.Contains(search));
            }

            // Apply role filter
            if (role != "all")
            {
                query = query.Where(cm => cm.Role.ToLower() == role.ToLower());
            }

            // Apply sorting
            switch (sort.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc" ? query.OrderBy(cm => cm.User.UserName) : query.OrderByDescending(cm => cm.User.UserName);
                    break;
                case "joined":
                default:
                    query = order.ToLower() == "asc" ? query.OrderBy(cm => cm.JoinedAt) : query.OrderByDescending(cm => cm.JoinedAt);
                    break;
            }

            // Apply pagination
            var members = await query
                .Skip((page - 1) * 20)
                .Take(20)
                .Select(cm => new discussionspot9.Models.ViewModels.MemberViewModel
                {
                    UserId = cm.UserId,
                    DisplayName = cm.User.UserName ?? "Unknown",
                    Email = cm.User.Email,
                    Role = cm.Role,
                    JoinedAt = cm.JoinedAt,
                    FormattedJoinedAt = FormatTimeAgo(cm.JoinedAt),
                    PostCount = _context.Posts.Count(p => p.UserId == cm.UserId && p.CommunityId == communityId),
                    CommentCount = _context.Comments.Count(c => c.UserId == cm.UserId && c.Post.CommunityId == communityId),
                    Karma = 0, // TODO: Implement karma calculation when Votes table exists
                    IsOnline = false, // TODO: Implement online status
                    IsBanned = false, // TODO: Implement ban status
                    Initials = GetInitials(cm.User.UserName ?? "U")
                })
                .ToListAsync();

            return members;
        }

        /// <summary>
        /// Get total members count for pagination
        /// </summary>
        private async Task<int> GetTotalMembersCount(int communityId, string search, string role)
        {
            var query = _context.CommunityMembers
                .Where(cm => cm.CommunityId == communityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(cm => cm.User.UserName.Contains(search) || 
                                        cm.User.Email.Contains(search));
            }

            if (role != "all")
            {
                query = query.Where(cm => cm.Role.ToLower() == role.ToLower());
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Load community rules
        /// </summary>
        private async Task<List<RuleViewModel>> LoadCommunityRules(int communityId)
        {
            // For now, return sample rules. In a real implementation, you'd have a CommunityRules table
            return new List<RuleViewModel>
            {
                new RuleViewModel
                {
                    RuleId = 1,
                    CommunityId = communityId,
                    Title = "Be respectful",
                    Description = "Treat all community members with respect and kindness. No harassment, bullying, or personal attacks.",
                    DisplayOrder = 1,
                    Severity = RuleSeverity.Critical,
                    Icon = "fas fa-heart",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "Admin",
                    UpdatedBy = "Admin"
                },
                new RuleViewModel
                {
                    RuleId = 2,
                    CommunityId = communityId,
                    Title = "Stay on topic",
                    Description = "Keep discussions relevant to the community's purpose. Off-topic posts may be removed.",
                    DisplayOrder = 2,
                    Severity = RuleSeverity.Important,
                    Icon = "fas fa-bullseye",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    UpdatedAt = DateTime.Now.AddDays(-25),
                    CreatedBy = "Admin",
                    UpdatedBy = "Admin"
                },
                new RuleViewModel
                {
                    RuleId = 3,
                    CommunityId = communityId,
                    Title = "No spam",
                    Description = "No excessive self-promotion, advertising, or spam. Keep promotional content to a minimum.",
                    DisplayOrder = 3,
                    Severity = RuleSeverity.Important,
                    Icon = "fas fa-ban",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    UpdatedAt = DateTime.Now.AddDays(-20),
                    CreatedBy = "Admin",
                    UpdatedBy = "Admin"
                }
            };
        }

        /// <summary>
        /// Load rule templates
        /// </summary>
        private async Task<List<RuleTemplateViewModel>> LoadRuleTemplates()
        {
            return new List<RuleTemplateViewModel>
            {
                new RuleTemplateViewModel
                {
                    TemplateId = 1,
                    Title = "No NSFW Content",
                    Description = "No adult, sexual, or inappropriate content is allowed in this community.",
                    Category = "Content",
                    Severity = RuleSeverity.Critical,
                    Icon = "fas fa-exclamation-triangle",
                    IsPopular = true
                },
                new RuleTemplateViewModel
                {
                    TemplateId = 2,
                    Title = "No Personal Information",
                    Description = "Do not share personal information such as addresses, phone numbers, or private details.",
                    Category = "Privacy",
                    Severity = RuleSeverity.Critical,
                    Icon = "fas fa-shield-alt",
                    IsPopular = true
                },
                new RuleTemplateViewModel
                {
                    TemplateId = 3,
                    Title = "Use Descriptive Titles",
                    Description = "Post titles should be clear and descriptive to help others understand your content.",
                    Category = "Posting",
                    Severity = RuleSeverity.General,
                    Icon = "fas fa-heading",
                    IsPopular = false
                }
            };
        }

        /// <summary>
        /// Load analytics data
        /// </summary>
        private async Task LoadAnalyticsData(CommunityAnalyticsViewModel model)
        {
            // Sample data - in a real implementation, you'd query actual data
            model.TotalMembers = await _context.CommunityMembers.CountAsync(cm => cm.CommunityId == model.CommunityId);
            model.TotalPosts = await _context.Posts.CountAsync(p => p.CommunityId == model.CommunityId);
            model.TotalComments = await _context.Comments.CountAsync(c => c.Post.CommunityId == model.CommunityId);
            model.TotalViews = 0; // TODO: Implement view tracking
            model.EngagementRate = 0.75; // Sample data
            model.GrowthRate = 0.12; // Sample data

            // Generate sample time series data
            var days = (model.EndDate - model.StartDate).Days;
            for (int i = 0; i <= days; i++)
            {
                var date = model.StartDate.AddDays(i);
                model.MemberGrowth.Add(new AnalyticsDataPoint
                {
                    Date = date,
                    Value = Random.Shared.Next(0, 10),
                    FormattedDate = date.ToString("MMM dd")
                });
                model.PostActivity.Add(new AnalyticsDataPoint
                {
                    Date = date,
                    Value = Random.Shared.Next(0, 20),
                    FormattedDate = date.ToString("MMM dd")
                });
            }

            // Load top contributors
            model.TopContributors = await _context.CommunityMembers
                .Where(cm => cm.CommunityId == model.CommunityId)
                .Include(cm => cm.User)
                .OrderByDescending(cm => cm.JoinedAt)
                .Take(5)
                .Select(cm => new TopContributorViewModel
                {
                    UserId = cm.UserId,
                    DisplayName = cm.User.UserName ?? "Unknown",
                    Initials = GetInitials(cm.User.UserName ?? "U"),
                    PostCount = _context.Posts.Count(p => p.UserId == cm.UserId && p.CommunityId == model.CommunityId),
                    CommentCount = _context.Comments.Count(c => c.UserId == cm.UserId && c.Post.CommunityId == model.CommunityId),
                    TotalKarma = 0, // TODO: Calculate karma
                    Role = cm.Role
                })
                .ToListAsync();
        }

        /// <summary>
        /// Load filter data
        /// </summary>
        private async Task LoadFilterData(ContentFiltersViewModel model)
        {
            // Sample banned words
            model.BannedWords = new List<BannedWordViewModel>
            {
                new BannedWordViewModel
                {
                    Id = 1,
                    Word = "spam",
                    CaseSensitive = false,
                    WholeWordOnly = true,
                    Action = "remove",
                    CreatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "Admin",
                    UsageCount = 5
                },
                new BannedWordViewModel
                {
                    Id = 2,
                    Word = "scam",
                    CaseSensitive = false,
                    WholeWordOnly = true,
                    Action = "flag",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "Moderator",
                    UsageCount = 2
                }
            };

            // Sample filter logs
            model.RecentLogs = new List<FilterLogViewModel>
            {
                new FilterLogViewModel
                {
                    Id = 1,
                    ContentType = "post",
                    ContentId = "123",
                    AuthorName = "User123",
                    FilterReason = "Contains banned word: spam",
                    Action = "removed",
                    FilteredAt = DateTime.Now.AddHours(-2),
                    FilteredBy = "AutoMod",
                    ContentPreview = "This is a spam post..."
                }
            };

            model.TotalFilteredItems = 15;
            model.FilteredToday = 3;
            model.FilteredThisWeek = 8;
        }

        /// <summary>
        /// Format time ago string
        /// </summary>
        private string FormatTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalDays >= 365)
                return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
            if (timeSpan.TotalDays >= 30)
                return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day(s) ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            
            return "Just now";
        }

        /// <summary>
        /// Get user initials
        /// </summary>
        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "U";
            
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2)
                return $"{words[0][0]}{words[1][0]}".ToUpper();
            
            return name.Length >= 2 ? name.Substring(0, 2).ToUpper() : name.ToUpper();
        }
    }
}