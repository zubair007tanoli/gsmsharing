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
        public async Task<IActionResult> Index(string sort = "trending", string category = null, int page = 1)
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
    }
}