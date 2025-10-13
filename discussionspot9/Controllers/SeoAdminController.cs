using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.AdminViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    [Authorize] // Add role check if needed
    [Route("admin/seo")]
    public class SeoAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SmartPostSelectorService _selectorService;
        private readonly MultiSiteAdSenseService _multiSiteAdsenseService;
        private readonly GoogleSearchConsoleService _searchConsoleService;
        private readonly EmailNotificationService _emailService;
        private readonly ILogger<SeoAdminController> _logger;

        public SeoAdminController(
            ApplicationDbContext context,
            SmartPostSelectorService selectorService,
            MultiSiteAdSenseService multiSiteAdsenseService,
            GoogleSearchConsoleService searchConsoleService,
            EmailNotificationService emailService,
            ILogger<SeoAdminController> logger)
        {
            _context = context;
            _selectorService = selectorService;
            _multiSiteAdsenseService = multiSiteAdsenseService;
            _searchConsoleService = searchConsoleService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            // Get revenue from both sites
            var totalRevenue = await _multiSiteAdsenseService.GetTotalRevenueAsync(startDate, endDate);
            var todayRevenue = await _multiSiteAdsenseService.GetTotalRevenueAsync(endDate, endDate);
            var revenueBySite = await _multiSiteAdsenseService.GetRevenueBySiteAsync(startDate, endDate);

            // Get top earning posts across all sites
            var topEarningData = await _multiSiteAdsenseService.GetTopEarningPostsAsync(10, 30);
            var topEarningPosts = new List<TopEarningPost>();

            foreach (var postData in topEarningData)
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postData.PostId);

                if (post != null)
                {
                    topEarningPosts.Add(new TopEarningPost
                    {
                        PostId = post.PostId,
                        Title = post.Title,
                        Slug = post.Slug,
                        CommunitySlug = post.Community?.Slug ?? "",
                        Earnings = postData.TotalEarnings,
                        Views = postData.TotalPageViews,
                        RPM = postData.AverageRPM,
                        SiteDomain = postData.SiteDomain
                    });
                }
            }

            var model = new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                RevenueBySite = revenueBySite,
                PendingOptimizations = await _context.PostSeoQueues.CountAsync(q => q.Status == "Pending"),
                CompletedOptimizations = await _context.SeoOptimizationLogs.CountAsync(l => l.OptimizedAt >= startDate),
                TotalPosts = await _context.Posts.CountAsync(p => p.Status == "published"),
                TopEarningPosts = topEarningPosts
            };

            return View(model);
        }

        [HttpGet("queue")]
        public async Task<IActionResult> OptimizationQueue()
        {
            var queue = await _context.PostSeoQueues
                .Include(q => q.Post)
                .Where(q => q.Status == "Pending" || q.Status == "Processing")
                .OrderBy(q => q.Priority)
                .ThenByDescending(q => q.EstimatedRevenueImpact)
                .ToListAsync();

            return View(queue);
        }

        [HttpGet("history")]
        public async Task<IActionResult> OptimizationHistory()
        {
            var history = await _context.SeoOptimizationLogs
                .Include(l => l.Post)
                .OrderByDescending(l => l.OptimizedAt)
                .Take(100)
                .ToListAsync();

            return View(history);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue()
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            var revenue = await _context.AdSenseRevenues
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var topEarningPostIds = await _multiSiteAdsenseService.GetTopEarningPostsAsync(20, 30);
            var topEarningPosts = new List<discussionspot9.Models.ViewModels.AdminViewModels.TopEarningPost>();

            foreach (var postData in topEarningPostIds)
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postData.PostId);

                if (post != null)
                {
                    topEarningPosts.Add(new discussionspot9.Models.ViewModels.AdminViewModels.TopEarningPost
                    {
                        PostId = post.PostId,
                        Title = post.Title,
                        Slug = post.Slug,
                        CommunitySlug = post.Community?.Slug ?? "",
                        SiteDomain = postData.SiteDomain,
                        Earnings = postData.TotalEarnings,
                        Views = postData.TotalPageViews,
                        RPM = postData.AverageRPM
                    });
                }
            }

            var model = new RevenueViewModel
            {
                RevenueData = revenue,
                TopEarningPosts = topEarningPosts,
                TotalRevenue = revenue.Sum(r => r.Earnings),
                AvgDailyRevenue = revenue.Any() ? revenue.Average(r => r.Earnings) : 0
            };

            return View(model);
        }

        [HttpPost("approve-optimization/{id}")]
        public async Task<IActionResult> ApproveOptimization(int id)
        {
            var queueItem = await _context.PostSeoQueues.FindAsync(id);
            
            if (queueItem == null)
                return NotFound();

            queueItem.RequiresApproval = false;
            queueItem.Status = "Pending"; // Will be picked up by next optimization cycle
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Optimization approved and queued for processing";
            return RedirectToAction(nameof(OptimizationQueue));
        }

        [HttpPost("reject-optimization/{id}")]
        public async Task<IActionResult> RejectOptimization(int id)
        {
            var queueItem = await _context.PostSeoQueues.FindAsync(id);
            
            if (queueItem == null)
                return NotFound();

            queueItem.Status = "Skipped";
            queueItem.ProcessedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Optimization rejected";
            return RedirectToAction(nameof(OptimizationQueue));
        }

        [HttpPost("sync-adsense")]
        public async Task<IActionResult> SyncAdSense()
        {
            try
            {
                await _multiSiteAdsenseService.SyncAllSitesRevenueAsync();
                TempData["SuccessMessage"] = "AdSense data synced successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to sync: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("sync-search-console")]
        public async Task<IActionResult> SyncSearchConsole()
        {
            try
            {
                await _searchConsoleService.SyncAllPostsPerformanceAsync();
                TempData["SuccessMessage"] = "Search Console data synced successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to sync: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                await _emailService.SendWeeklyOptimizationSummaryAsync();
                TempData["SuccessMessage"] = "Test email sent successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to send: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet("trending-queries")]
        public async Task<IActionResult> TrendingQueries()
        {
            var queries = await _searchConsoleService.GetTrendingQueriesAsync(7, 50);
            return View(queries);
        }

        [HttpGet("declining-pages")]
        public async Task<IActionResult> DecliningPages()
        {
            var pages = await _searchConsoleService.GetDecliningPagesAsync(14);
            return View(pages);
        }
    }
}

