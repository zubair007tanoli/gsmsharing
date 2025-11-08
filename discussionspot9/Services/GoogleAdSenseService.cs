using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Services
{
    public class GoogleAdSenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GoogleAdSenseService> _logger;
        private readonly MultiSiteAdSenseService _multiSiteAdSenseService;

        public GoogleAdSenseService(
            ApplicationDbContext context,
            ILogger<GoogleAdSenseService> logger,
            MultiSiteAdSenseService multiSiteAdSenseService)
        {
            _context = context;
            _logger = logger;
            _multiSiteAdSenseService = multiSiteAdSenseService;
        }

        /// <summary>
        /// Sync daily revenue data from AdSense (delegates to multi-site service).
        /// </summary>
        public async Task<bool> SyncDailyRevenueAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.AddDays(-1).Date;

            try
            {
                var success = await _multiSiteAdSenseService.SyncAllSitesRevenueAsync(targetDate);

                var siteWideRecord = await _context.AdSenseRevenues
                    .FirstOrDefaultAsync(r => r.Date == targetDate && r.PostId == null);

                if (siteWideRecord == null)
                {
                    _logger.LogWarning("AdSense sync completed for {Date}, but no aggregated revenue record was stored.", targetDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    _logger.LogInformation("✅ AdSense revenue stored for {Date}: ${Earnings:F2}", targetDate.ToString("yyyy-MM-dd"), siteWideRecord.Earnings);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to sync AdSense revenue for {Date}", targetDate.ToString("yyyy-MM-dd"));
                return false;
            }
        }

        /// <summary>
        /// Get total earnings for date range.
        /// </summary>
        public async Task<decimal> GetEarningsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AdSenseRevenues
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .SumAsync(a => a.Earnings);
        }

        /// <summary>
        /// Get top earning posts.
        /// </summary>
        public async Task<List<(int PostId, decimal Earnings)>> GetTopEarningPostsAsync(int count = 10, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            return await _context.AdSenseRevenues
                .Where(a => a.PostId != null && a.Date >= startDate)
                .GroupBy(a => a.PostId)
                .Select(g => new { PostId = g.Key!.Value, TotalEarnings = g.Sum(a => a.Earnings) })
                .OrderByDescending(x => x.TotalEarnings)
                .Take(count)
                .Select(x => ValueTuple.Create(x.PostId, x.TotalEarnings))
                .ToListAsync();
        }

        /// <summary>
        /// Calculate RPM (Revenue Per Mille/1000 views).
        /// </summary>
        public async Task<decimal> CalculateRPMAsync(int postId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            var stats = await _context.AdSenseRevenues
                .Where(a => a.PostId == postId && a.Date >= startDate)
                .GroupBy(a => a.PostId)
                .Select(g => new
                {
                    TotalRevenue = g.Sum(a => a.Earnings),
                    TotalViews = g.Sum(a => a.PageViews)
                })
                .FirstOrDefaultAsync();

            if (stats == null || stats.TotalViews == 0)
            {
                return 0;
            }

            return (stats.TotalRevenue / stats.TotalViews) * 1000;
        }
    }
}
