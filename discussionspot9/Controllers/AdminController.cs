using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    [Authorize]  // Just require login, check admin in action
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IPresenceService _presenceService;
        private readonly IConfiguration _configuration;
        private readonly Services.MCP.IMcpServerManager? _mcpServerManager;
        private readonly IWebHostEnvironment? _environment;

        public AdminController(
            ApplicationDbContext context, 
            ILogger<AdminController> logger,
            IAnnouncementRepository announcementRepository,
            IPresenceService presenceService,
            IConfiguration configuration,
            Services.MCP.IMcpServerManager? mcpServerManager = null,
            IWebHostEnvironment? environment = null)
        {
            _context = context;
            _logger = logger;
            _announcementRepository = announcementRepository;
            _presenceService = presenceService;
            _configuration = configuration;
            _mcpServerManager = mcpServerManager;
            _environment = environment;
        }

        [HttpGet("dashboard")]
        [HttpGet("")]
        public async Task<IActionResult> Dashboard()
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access the admin area.";
                return RedirectToAction("AccessDenied", "Account");
            }
            
            ViewData["Title"] = "Admin Dashboard";
            ViewData["PageTitle"] = "Dashboard";
            ViewData["PageDescription"] = "Overview of your platform's key metrics and recent activity";
            
            _logger.LogInformation("Admin dashboard accessed by {Email}", userEmail);
            
            return View();
        }

        [HttpGet("stats/test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestStats()
        {
            var results = new Dictionary<string, object>();
            
            try { results["Users"] = await _context.Users.CountAsync(); } 
            catch (Exception ex) { results["Users"] = $"ERROR: {ex.Message}"; }
            
            try { results["Posts"] = await _context.Posts.CountAsync(); }
            catch (Exception ex) { results["Posts"] = $"ERROR: {ex.Message}"; }
            
            try { results["Communities"] = await _context.Communities.CountAsync(); }
            catch (Exception ex) { results["Communities"] = $"ERROR: {ex.Message}"; }
            
            try { results["PostReports"] = await _context.PostReports.CountAsync(); }
            catch (Exception ex) { results["PostReports"] = $"ERROR: {ex.Message}"; }
            
            try { results["UserPresences"] = await _context.UserPresences.CountAsync(); }
            catch (Exception ex) { results["UserPresences"] = $"ERROR: {ex.Message}"; }
            
            try { results["AdSenseRevenues"] = await _context.AdSenseRevenues.CountAsync(); }
            catch (Exception ex) { results["AdSenseRevenues"] = $"ERROR: {ex.Message}"; }
            
            try { results["PostSeoQueues"] = await _context.PostSeoQueues.CountAsync(); }
            catch (Exception ex) { results["PostSeoQueues"] = $"ERROR: {ex.Message}"; }
            
            return Json(results);
        }

        [HttpGet("stats")]
        [AllowAnonymous] // Temporarily allow for debugging, remove after testing
        public async Task<IActionResult> GetDashboardStats()
        {
            _logger.LogInformation("GetDashboardStats called by {User}", User.Identity?.Name);
            
            try
            {
                var now = DateTime.UtcNow;
                var thirtyDaysAgo = now.AddDays(-30);
                var lastMonth = now.AddMonths(-1);

                // Get current counts with individual try-catch
                int totalUsers = 0;
                int totalPosts = 0;
                int totalCommunities = 0;
                int pendingReports = 0;
                int onlineUsers = 0;
                double userGrowth = 0;
                double postGrowth = 0;
                double monthlyRevenue = 0;
                double revenueGrowth = 0;
                int seoQueueCount = 0;

                try
                {
                    totalUsers = await _context.Users.CountAsync();
                    _logger.LogInformation("Total users: {Count}", totalUsers);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error counting users");
                }

                try
                {
                    totalPosts = await _context.Posts.CountAsync();
                    _logger.LogInformation("Total posts: {Count}", totalPosts);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error counting posts");
                }

                try
                {
                    totalCommunities = await _context.Communities.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error counting communities");
                }

                try
                {
                    pendingReports = await _context.PostReports.CountAsync(r => r.Status == "pending");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error counting reports");
                }

                // Get growth percentages safely
                try
                {
                    var lastMonthUsers = await _context.UserProfiles.CountAsync(u => u.JoinDate < lastMonth);
                    userGrowth = lastMonthUsers > 0 ? ((double)(totalUsers - lastMonthUsers) / lastMonthUsers * 100) : 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating user growth");
                }

                try
                {
                    var lastMonthPosts = await _context.Posts.CountAsync(p => p.CreatedAt < lastMonth);
                    postGrowth = lastMonthPosts > 0 ? ((double)(totalPosts - lastMonthPosts) / lastMonthPosts * 100) : 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating post growth");
                }

                // Get revenue safely
                try
                {
                    monthlyRevenue = await _context.AdSenseRevenues
                        .Where(a => a.Date >= lastMonth)
                        .SumAsync(a => (double?)a.Earnings) ?? 0;

                    var previousMonthRevenue = await _context.AdSenseRevenues
                        .Where(a => a.Date >= lastMonth.AddMonths(-1) && a.Date < lastMonth)
                        .SumAsync(a => (double?)a.Earnings) ?? 0;

                    revenueGrowth = previousMonthRevenue > 0 ? ((monthlyRevenue - previousMonthRevenue) / previousMonthRevenue * 100) : 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting revenue data");
                }

                // Get online users safely - Use PresenceService (no duplication!)
                try
                {
                    var onlineUserIds = await _presenceService.GetOnlineUserIdsAsync();
                    onlineUsers = onlineUserIds.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error counting online users");
                }

                // Get post type distribution
                var postTypeDistribution = new List<object>();
                try
                {
                    var postTypes = await _context.Posts
                        .GroupBy(p => p.PostType)
                        .Select(g => new { Type = g.Key, Count = g.Count() })
                        .ToListAsync();

                    postTypeDistribution = postTypes.Select(pt => new
                    {
                        type = pt.Type,
                        count = pt.Count,
                        percentage = totalPosts > 0 ? Math.Round((double)pt.Count / totalPosts * 100, 1) : 0
                    }).ToList<object>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting post type distribution");
                }

                // Get SEO queue count
                try
                {
                    seoQueueCount = await _context.PostSeoQueues
                        .CountAsync(q => q.Status == "pending");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PostSeoQueues table might not exist");
                }

                // Get revenue trend
                var revenueTrend = new List<object>();
                try
                {
                    revenueTrend = (await _context.AdSenseRevenues
                        .Where(a => a.Date >= thirtyDaysAgo)
                        .GroupBy(a => a.Date.Date)
                        .Select(g => new
                        {
                            date = g.Key,
                            revenue = g.Sum(a => (double)a.Earnings)
                        })
                        .OrderBy(x => x.date)
                        .ToListAsync()).ToList<object>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting revenue trend");
                }

                var stats = new
                {
                    totalUsers,
                    totalPosts,
                    totalCommunities,
                    pendingReports,
                    onlineUsers,
                    seoQueueCount,
                    userGrowth = Math.Round(userGrowth, 1),
                    postGrowth = Math.Round(postGrowth, 1),
                    monthlyRevenue = Math.Round(monthlyRevenue, 2),
                    revenueGrowth = Math.Round(revenueGrowth, 1),
                    postTypeDistribution,
                    revenueTrend
                };

                _logger.LogInformation("Stats successfully compiled: Users={Users}, Posts={Posts}", totalUsers, totalPosts);
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error getting dashboard stats");
                return Json(new { 
                    error = "Failed to load stats",
                    message = ex.Message,
                    totalUsers = 0,
                    totalPosts = 0,
                    totalCommunities = 0,
                    pendingReports = 0,
                    onlineUsers = 0,
                    seoQueueCount = 0,
                    userGrowth = 0,
                    postGrowth = 0,
                    monthlyRevenue = 0,
                    revenueGrowth = 0,
                    postTypeDistribution = new List<object>(),
                    revenueTrend = new List<object>()
                });
            }
        }

        [HttpGet("online-users")]
        [AllowAnonymous] // Temporarily allow for debugging
        public async Task<IActionResult> GetOnlineUsers()
        {
            _logger.LogInformation("GetOnlineUsers called");
            try
            {
                // Use PresenceService (no duplication!)
                var onlineUserIds = await _presenceService.GetOnlineUserIdsAsync();
                
                if (!onlineUserIds.Any())
                {
                    return Json(new { success = true, count = 0, users = new List<object>() });
                }

                // Get user details
                var onlineUsers = await _context.UserProfiles
                    .Where(u => onlineUserIds.Contains(u.UserId))
                    .OrderByDescending(u => u.LastActive)
                    .Take(20)
                    .Select(u => new
                    {
                        userId = u.UserId,
                        displayName = u.DisplayName,
                        avatarUrl = u.AvatarUrl,
                        lastSeen = u.LastActive,
                        currentPage = "", // Will be populated from UserPresences if needed
                        status = "online"
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    count = onlineUsers.Count,
                    users = onlineUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return Json(new { success = false, error = "Failed to load online users", message = ex.Message });
            }
        }

        [HttpGet("recent-activity")]
        [AllowAnonymous] // Temporarily allow for debugging
        public async Task<IActionResult> GetRecentActivity()
        {
            _logger.LogInformation("GetRecentActivity called");
            try
            {
                var activities = new List<object>();

                // Get recent users (last 24 hours)
                var recentUsers = await _context.UserProfiles
                    .OrderByDescending(u => u.JoinDate)
                    .Take(5)
                    .Select(u => new
                    {
                        type = "user_registered",
                        icon = "fa-user-plus",
                        color = "#667eea",
                        title = "New User Registration",
                        meta = u.DisplayName + " joined",
                        time = u.JoinDate
                    })
                    .ToListAsync();

                // Get recent posts
                var recentPosts = await _context.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new
                    {
                        type = "post_created",
                        icon = "fa-newspaper",
                        color = "#f5576c",
                        title = "New Post Created",
                        meta = "\"" + (p.Title.Length > 40 ? p.Title.Substring(0, 40) + "..." : p.Title) + "\"",
                        time = p.CreatedAt
                    })
                    .ToListAsync();

                // Get recent reports
                var recentReports = await _context.PostReports
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(3)
                    .Select(r => new
                    {
                        type = "post_reported",
                        icon = "fa-flag",
                        color = "#fa709a",
                        title = "Post Reported",
                        meta = r.Reason + " - " + r.Status,
                        time = r.CreatedAt
                    })
                    .ToListAsync();

                // Combine and sort by time
                activities.AddRange(recentUsers);
                activities.AddRange(recentPosts);
                activities.AddRange(recentReports);

                var sortedActivities = activities
                    .OrderByDescending(a => ((dynamic)a).time)
                    .Take(10)
                    .Select(a =>
                    {
                        var activity = (dynamic)a;
                        var timeAgo = GetTimeAgo(activity.time);
                        return new
                        {
                            type = activity.type,
                            icon = activity.icon,
                            color = activity.color,
                            title = activity.title,
                            meta = activity.meta + " • " + timeAgo
                        };
                    })
                    .ToList();

                return Json(new { success = true, activities = sortedActivities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity");
                return Json(new { success = false, error = "Failed to load activity" });
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
            return dateTime.ToString("MMM dd, yyyy");
        }

        #region Announcement Management

        [HttpGet("announcements")]
        public async Task<IActionResult> Announcements()
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access the admin area.";
                return RedirectToAction("Index", "Home");
            }

            var announcements = await _announcementRepository.GetAllAnnouncementsAsync();
            return View(announcements);
        }

        [HttpGet("announcements/create")]
        public IActionResult CreateAnnouncement()
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access the admin area.";
                return RedirectToAction("Index", "Home");
            }

            return View(new AnnouncementViewModel());
        }

        [HttpPost("announcements/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(AnnouncementViewModel model)
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access the admin area.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var announcement = new Announcement
                {
                    Title = model.Title,
                    Message = model.Message,
                    Type = model.Type,
                    Icon = model.Icon,
                    LinkUrl = model.LinkUrl,
                    LinkText = model.LinkText,
                    IsActive = model.IsActive,
                    IsDismissible = model.IsDismissible,
                    Priority = model.Priority,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                };

                await _announcementRepository.CreateAnnouncementAsync(announcement);
                
                TempData["SuccessMessage"] = "Announcement created successfully!";
                return RedirectToAction(nameof(Announcements));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating announcement");
                ModelState.AddModelError("", "An error occurred while creating the announcement.");
                return View(model);
            }
        }

        [HttpGet("announcements/edit/{id}")]
        public async Task<IActionResult> EditAnnouncement(int id)
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access the admin area.";
                return RedirectToAction("Index", "Home");
            }

            var announcement = await _announcementRepository.GetAnnouncementByIdAsync(id);
            if (announcement == null)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToAction(nameof(Announcements));
            }

            var model = new AnnouncementViewModel
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Message = announcement.Message,
                Type = announcement.Type,
                Icon = announcement.Icon,
                LinkUrl = announcement.LinkUrl,
                LinkText = announcement.LinkText,
                IsActive = announcement.IsActive,
                IsDismissible = announcement.IsDismissible,
                Priority = announcement.Priority,
                StartDate = announcement.StartDate,
                EndDate = announcement.EndDate
            };

            return View(model);
        }

        [HttpPost("announcements/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnnouncement(int id, AnnouncementViewModel model)
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access the admin area.";
                return RedirectToAction("Index", "Home");
            }

            if (id != model.AnnouncementId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var announcement = await _announcementRepository.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    TempData["ErrorMessage"] = "Announcement not found.";
                    return RedirectToAction(nameof(Announcements));
                }

                announcement.Title = model.Title;
                announcement.Message = model.Message;
                announcement.Type = model.Type;
                announcement.Icon = model.Icon;
                announcement.LinkUrl = model.LinkUrl;
                announcement.LinkText = model.LinkText;
                announcement.IsActive = model.IsActive;
                announcement.IsDismissible = model.IsDismissible;
                announcement.Priority = model.Priority;
                announcement.StartDate = model.StartDate;
                announcement.EndDate = model.EndDate;

                await _announcementRepository.UpdateAnnouncementAsync(announcement);
                
                TempData["SuccessMessage"] = "Announcement updated successfully!";
                return RedirectToAction(nameof(Announcements));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement");
                ModelState.AddModelError("", "An error occurred while updating the announcement.");
                return View(model);
            }
        }

        [HttpPost("announcements/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var success = await _announcementRepository.DeleteAnnouncementAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Announcement deleted successfully" });
                }
                
                return Json(new { success = false, message = "Announcement not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting announcement");
                return Json(new { success = false, message = "An error occurred while deleting the announcement" });
            }
        }

        [HttpPost("announcements/toggle/{id}")]
        public async Task<IActionResult> ToggleAnnouncement(int id)
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var announcement = await _announcementRepository.GetAnnouncementByIdAsync(id);
                if (announcement == null)
                {
                    return Json(new { success = false, message = "Announcement not found" });
                }

                announcement.IsActive = !announcement.IsActive;
                await _announcementRepository.UpdateAnnouncementAsync(announcement);

                return Json(new { success = true, isActive = announcement.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling announcement");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        #endregion
        
        // ===============================================
        // DATABASE MANAGEMENT
        // ===============================================
        
        [HttpGet("database")]
        public async Task<IActionResult> Database()
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access this page.";
                return RedirectToAction("AccessDenied", "Account");
            }
            
            try
            {
                // Get database statistics
                var stats = new
                {
                    TotalUsers = await _context.UserProfiles.CountAsync(),
                    TotalPosts = await _context.Posts.CountAsync(),
                    TotalComments = await _context.Comments.CountAsync(),
                    TotalCommunities = await _context.Communities.CountAsync(),
                    TotalReports = await _context.PostReports.CountAsync(),
                    TotalActivities = await _context.UserActivities.CountAsync(),
                    TotalNotifications = await _context.Notifications.CountAsync(),
                    TotalMedia = await _context.Media.CountAsync(),
                    DatabaseName = _context.Database.GetDbConnection().Database,
                    ServerName = _context.Database.GetDbConnection().DataSource,
                    ConnectionState = _context.Database.GetDbConnection().State.ToString()
                };
                
                // Get table sizes
                var tableSizes = new Dictionary<string, long>
                {
                    { "Posts", stats.TotalPosts },
                    { "Comments", stats.TotalComments },
                    { "UserProfiles", stats.TotalUsers },
                    { "Communities", stats.TotalCommunities },
                    { "UserActivities", stats.TotalActivities },
                    { "Notifications", stats.TotalNotifications },
                    { "Media", stats.TotalMedia },
                    { "PostReports", stats.TotalReports }
                };
                
                ViewBag.Stats = stats;
                ViewBag.TableSizes = tableSizes;
                ViewData["Title"] = "Database Management";
                ViewData["PageTitle"] = "Database Management";
                ViewData["PageDescription"] = "View database statistics and manage database operations";
                
                return View("~/Views/AdminManagement/Database.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading database page");
                TempData["ErrorMessage"] = $"Error loading database information: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }
        
        // ===============================================
        // MCP SERVER STATUS
        // ===============================================
        
        [HttpGet("mcp-status")]
        public async Task<IActionResult> McpStatus()
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                TempData["ErrorMessage"] = "You don't have permission to access this page.";
                return RedirectToAction("AccessDenied", "Account");
            }
            
            // Get endpoints from configuration
            var seoEndpoint = _configuration["MCP:Servers:SeoAutomation:Endpoint"] ?? "http://localhost:5001";
            var perfEndpoint = _configuration["MCP:Servers:Performance:Endpoint"] ?? "http://localhost:5002";
            var prefsEndpoint = _configuration["MCP:Servers:UserPreferences:Endpoint"] ?? "http://localhost:5003";
            
            var seoStatus = await CheckMcpServerStatusAsync($"{seoEndpoint}/health");
            var perfStatus = await CheckMcpServerStatusAsync($"{perfEndpoint}/health");
            var prefsStatus = await CheckMcpServerStatusAsync($"{prefsEndpoint}/health");
            
            var status = new
            {
                SeoAutomation = new { 
                    seoStatus.IsOnline, 
                    seoStatus.StatusCode, 
                    seoStatus.ResponseTime, 
                    seoStatus.ResponseTimeMs,
                    seoStatus.Message,
                    Endpoint = seoEndpoint
                },
                Performance = new { 
                    perfStatus.IsOnline, 
                    perfStatus.StatusCode, 
                    perfStatus.ResponseTime, 
                    perfStatus.ResponseTimeMs,
                    perfStatus.Message,
                    Endpoint = perfEndpoint
                },
                UserPreferences = new { 
                    prefsStatus.IsOnline, 
                    prefsStatus.StatusCode, 
                    prefsStatus.ResponseTime, 
                    prefsStatus.ResponseTimeMs,
                    prefsStatus.Message,
                    Endpoint = prefsEndpoint
                },
                LastChecked = DateTime.UtcNow
            };
            
            // Get diagnostic information
            var diagnostics = new
            {
                AutoStartEnabled = _configuration.GetValue<bool>("MCP:AutoStart:Enabled", true),
                PythonPath = _configuration["Python:ExecutablePath"] ?? "python",
                SeoServerEnabled = _configuration.GetValue<bool>("MCP:Servers:SeoAutomation:Enabled", true),
                ManagerAvailable = _mcpServerManager != null
            };

            ViewBag.McpStatus = status;
            ViewBag.Diagnostics = diagnostics;
            ViewData["Title"] = "MCP Server Status";
            ViewData["PageTitle"] = "MCP Server Status";
            ViewData["PageDescription"] = "Monitor MCP server health and status";
            
            return View("~/Views/AdminManagement/McpStatus.cshtml");
        }

        [HttpPost("mcp-status/start-server")]
        public async Task<IActionResult> StartMcpServer([FromBody] StartServerRequest request)
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (_mcpServerManager == null)
            {
                return Json(new { success = false, message = "MCP Server Manager not available" });
            }

            string serverName = string.Empty;
            try
            {
                var servers = new Dictionary<string, (string Script, int Port)>
                {
                    { "SeoAutomation", ("seo-automation/main.py", 5001) }
                };

                serverName = request?.ServerName ?? string.Empty;
                if (string.IsNullOrEmpty(serverName))
                {
                    return Json(new { success = false, message = "Server name is required" });
                }

                if (!servers.ContainsKey(serverName))
                {
                    return Json(new { success = false, message = $"Unknown server: {serverName}" });
                }

                var (script, port) = servers[serverName];
                
                // Try multiple possible paths (including Ubuntu deployment paths)
                var possiblePaths = new List<string>
                {
                    // Standard paths
                    Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", script),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", script),
                    // Ubuntu deployment paths
                    "/var/www/discussionspot/mcp-servers/" + script,
                    "/var/www/discussionspot9/mcp-servers/" + script
                };

                // Try parent directories if we're in bin/Debug or bin/Release
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                if (baseDir.Contains("bin"))
                {
                    var projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;
                    if (!string.IsNullOrEmpty(projectRoot))
                    {
                        possiblePaths.Add(Path.Combine(projectRoot, "mcp-servers", script));
                    }
                }

                // Try ContentRootPath
                if (_environment != null)
                {
                    possiblePaths.Add(Path.Combine(_environment.ContentRootPath, "mcp-servers", script));
                    var contentRootParent = Directory.GetParent(_environment.ContentRootPath)?.FullName;
                    if (!string.IsNullOrEmpty(contentRootParent))
                    {
                        possiblePaths.Add(Path.Combine(contentRootParent, "discussionspot9", "mcp-servers", script));
                        possiblePaths.Add(Path.Combine(contentRootParent, "mcp-servers", script));
                    }
                }

                string? scriptPath = null;
                foreach (var path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        scriptPath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(scriptPath))
                {
                    return Json(new { success = false, message = $"Server script not found. Searched: {string.Join(", ", possiblePaths)}" });
                }

                var result = await _mcpServerManager.StartServerAsync(serverName, scriptPath, port);
                
                if (result)
                {
                    return Json(new { success = true, message = $"{serverName} started successfully" });
                }
                else
                {
                    return Json(new { success = false, message = $"Failed to start {serverName}. Check logs for details." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MCP server {ServerName}", serverName ?? "Unknown");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("mcp-status/diagnostics")]
        public IActionResult McpDiagnostics()
        {
            // Check if user is admin
            var userEmail = User.Identity?.Name;
            if (!User.IsInRole("Admin") && userEmail != "zubair007tanoli@gmail.com")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var diagnostics = new
            {
                PythonInstalled = CheckPythonInstalled(),
                PythonVersion = GetPythonVersion(),
                ServerScriptExists = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", "seo-automation", "main.py")),
                ServerScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "mcp-servers", "seo-automation", "main.py"),
                ContentRootPath = _environment?.ContentRootPath,
                BaseDirectory = AppDomain.CurrentDomain.BaseDirectory,
                CurrentDirectory = Directory.GetCurrentDirectory(),
                AutoStartEnabled = _configuration.GetValue<bool>("MCP:AutoStart:Enabled", true),
                SeoServerEnabled = _configuration.GetValue<bool>("MCP:Servers:SeoAutomation:Enabled", true),
                GoogleAuthConfigured = !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientId"]) || 
                                      System.IO.File.Exists(Path.Combine(_environment?.ContentRootPath ?? "", "Secrets", "AuthKeys.json"))
            };

            return Json(diagnostics);
        }

        private bool CheckPythonInstalled()
        {
            try
            {
                var pythonExe = _configuration["Python:ExecutablePath"] ?? "python";
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(2000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private string GetPythonVersion()
        {
            try
            {
                var pythonExe = _configuration["Python:ExecutablePath"] ?? "python";
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var version = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);
                return version.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }
        
        private async Task<McpServerStatus> CheckMcpServerStatusAsync(string endpoint)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync(endpoint);
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                string message;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    message = $"Online ({responseTime:F0}ms)";
                }
                else
                {
                    message = $"Error: {response.StatusCode} ({responseTime:F0}ms)";
                }
                
                return new McpServerStatus
                {
                    IsOnline = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    ResponseTime = DateTime.UtcNow,
                    ResponseTimeMs = (int)responseTime,
                    Message = message
                };
            }
            catch (TaskCanceledException)
            {
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                return new McpServerStatus
                {
                    IsOnline = false,
                    StatusCode = 0,
                    ResponseTime = DateTime.UtcNow,
                    ResponseTimeMs = (int)responseTime,
                    Message = $"Timeout after {responseTime:F0}ms"
                };
            }
            catch (HttpRequestException ex)
            {
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                return new McpServerStatus
                {
                    IsOnline = false,
                    StatusCode = 0,
                    ResponseTime = DateTime.UtcNow,
                    ResponseTimeMs = (int)responseTime,
                    Message = $"Connection failed: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                return new McpServerStatus
                {
                    IsOnline = false,
                    StatusCode = 0,
                    ResponseTime = DateTime.UtcNow,
                    ResponseTimeMs = (int)responseTime,
                    Message = $"Error: {ex.GetType().Name} - {ex.Message}"
                };
            }
        }
        
        public class McpServerStatus
        {
            public bool IsOnline { get; set; }
            public int StatusCode { get; set; }
            public DateTime ResponseTime { get; set; }
            public int ResponseTimeMs { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class StartServerRequest
        {
            public string ServerName { get; set; } = string.Empty;
        }
    }
}

