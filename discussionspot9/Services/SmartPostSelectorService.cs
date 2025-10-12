using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace discussionspot9.Services
{
    /// <summary>
    /// Intelligently selects posts for optimization based on revenue and performance
    /// </summary>
    public class SmartPostSelectorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SmartPostSelectorService> _logger;

        public SmartPostSelectorService(
            ApplicationDbContext context,
            ILogger<SmartPostSelectorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Select posts for optimization - prioritizes by revenue potential
        /// </summary>
        public async Task<List<PostOptimizationCandidate>> SelectPostsForOptimizationAsync(int maxPosts = 50)
        {
            _logger.LogInformation("🎯 Selecting posts for optimization...");

            var candidates = new List<PostOptimizationCandidate>();
            var cutoffDate = DateTime.UtcNow.AddDays(-14); // Posts must be at least 14 days old

            // Get posts with performance data
            var posts = await _context.Posts
                .Where(p => p.Status == "published" && p.CreatedAt < cutoffDate)
                .Select(p => new
                {
                    p.PostId,
                    p.Title,
                    p.Slug,
                    p.CommentCount,
                    p.ViewCount,
                    p.CreatedAt
                })
                .ToListAsync();

            foreach (var post in posts)
            {
                try
                {
                    var candidate = await AnalyzePostAsync(post.PostId, post.Title, post.Slug, post.CommentCount, post.ViewCount);
                    
                    if (candidate != null && candidate.ShouldOptimize)
                    {
                        candidates.Add(candidate);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to analyze post {PostId}", post.PostId);
                }
            }

            // Sort by priority and revenue potential
            var selected = candidates
                .OrderBy(c => c.Priority)
                .ThenByDescending(c => c.EstimatedRevenueImpact)
                .Take(maxPosts)
                .ToList();

            _logger.LogInformation("✅ Selected {Count} posts for optimization", selected.Count);
            
            return selected;
        }

        private async Task<PostOptimizationCandidate?> AnalyzePostAsync(
            int postId, 
            string title, 
            string slug,
            int commentCount,
            int viewCount)
        {
            // Safety rules - NEVER optimize these
            if (commentCount >= 50)
            {
                _logger.LogDebug("Skipping post {PostId} - too many comments ({Count})", postId, commentCount);
                return null;
            }

            // Check if recently optimized
            var lastOptimization = await _context.SeoOptimizationLogs
                .Where(l => l.PostId == postId)
                .OrderByDescending(l => l.OptimizedAt)
                .FirstOrDefaultAsync();

            if (lastOptimization != null && lastOptimization.OptimizedAt > DateTime.UtcNow.AddDays(-21))
            {
                _logger.LogDebug("Skipping post {PostId} - optimized recently", postId);
                return null;
            }

            // Get performance metrics (last 30 days)
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            var recentPerformance = await _context.PostPerformanceMetrics
                .Where(m => m.PostId == postId && m.Date >= startDate && m.Date <= endDate)
                .ToListAsync();

            var recentRevenue = await _context.AdSenseRevenues
                .Where(a => a.PostId == postId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            // Calculate metrics
            var totalRevenue = recentRevenue.Sum(r => r.Earnings);
            var avgDailyRevenue = recentRevenue.Any() ? totalRevenue / recentRevenue.Count : 0;
            var totalViews = recentPerformance.Sum(p => p.Views);
            var avgSearchPosition = recentPerformance.Any(p => p.AvgSearchPosition > 0) 
                ? recentPerformance.Where(p => p.AvgSearchPosition > 0).Average(p => p.AvgSearchPosition)
                : 0;

            // Skip if earning too much (don't fix what's not broken)
            if (avgDailyRevenue > 5m)
            {
                _logger.LogDebug("Skipping post {PostId} - already earning well (${Revenue:F2}/day)", postId, avgDailyRevenue);
                return null;
            }

            // Skip if ranking in top 5
            if (avgSearchPosition > 0 && avgSearchPosition <= 5)
            {
                _logger.LogDebug("Skipping post {PostId} - already ranking well (position {Position:F1})", postId, avgSearchPosition);
                return null;
            }

            // Determine priority and reason
            var candidate = new PostOptimizationCandidate
            {
                PostId = postId,
                Title = title,
                Slug = slug,
                CurrentRevenue = totalRevenue,
                CurrentViews = totalViews,
                AvgSearchPosition = avgSearchPosition
            };

            // PRIORITY 1: High traffic but low revenue (biggest opportunity)
            if (totalViews > 1000 && avgDailyRevenue < 1m)
            {
                candidate.Priority = 1;
                candidate.Reason = "High traffic but low monetization";
                candidate.EstimatedRevenueImpact = totalViews * 0.005m; // Estimate $5 per 1000 views improvement
                candidate.ShouldOptimize = true;
            }
            // PRIORITY 2: Declining revenue
            else if (await IsRevenueDecliningAsync(postId, startDate))
            {
                candidate.Priority = 2;
                candidate.Reason = "Revenue declining";
                candidate.EstimatedRevenueImpact = totalRevenue * 0.3m; // Estimate 30% recovery
                candidate.ShouldOptimize = true;
            }
            // PRIORITY 3: Low traffic + some revenue (scale up winners)
            else if (totalViews < 500 && avgDailyRevenue > 0.5m)
            {
                candidate.Priority = 3;
                candidate.Reason = "Low traffic high RPM - scale opportunity";
                candidate.EstimatedRevenueImpact = avgDailyRevenue * 30 * 2; // Double traffic = double revenue
                candidate.ShouldOptimize = true;
            }
            // PRIORITY 4: Poor search ranking
            else if (avgSearchPosition > 10)
            {
                candidate.Priority = 4;
                candidate.Reason = "Poor search ranking";
                candidate.EstimatedRevenueImpact = 50m; // Conservative estimate
                candidate.ShouldOptimize = true;
            }

            return candidate;
        }

        private async Task<bool> IsRevenueDecliningAsync(int postId, DateTime startDate)
        {
            var revenues = await _context.AdSenseRevenues
                .Where(a => a.PostId == postId && a.Date >= startDate)
                .OrderBy(a => a.Date)
                .Select(a => new { a.Date, a.Earnings })
                .ToListAsync();

            if (revenues.Count < 14)
                return false;

            var firstWeek = revenues.Take(7).Sum(r => r.Earnings);
            var lastWeek = revenues.TakeLast(7).Sum(r => r.Earnings);

            if (firstWeek == 0)
                return false;

            var decline = ((firstWeek - lastWeek) / firstWeek) * 100;
            return decline >= 30; // 30% decline
        }

        /// <summary>
        /// Add posts to optimization queue
        /// </summary>
        public async Task QueuePostsForOptimizationAsync(List<PostOptimizationCandidate> candidates)
        {
            foreach (var candidate in candidates)
            {
                // Check if already in queue
                var existingQueue = await _context.PostSeoQueues
                    .FirstOrDefaultAsync(q => q.PostId == candidate.PostId && q.Status == "Pending");

                if (existingQueue != null)
                    continue;

                var queueItem = new PostSeoQueue
                {
                    PostId = candidate.PostId,
                    Priority = candidate.Priority,
                    Reason = candidate.Reason,
                    EstimatedRevenueImpact = candidate.EstimatedRevenueImpact,
                    AddedAt = DateTime.UtcNow,
                    Status = "Pending",
                    RequiresApproval = candidate.Priority <= 2 || candidate.EstimatedRevenueImpact > 100 // High-value changes need approval
                };

                _context.PostSeoQueues.Add(queueItem);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Queued {Count} posts for optimization", candidates.Count);
        }
    }

    public class PostOptimizationCandidate
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal EstimatedRevenueImpact { get; set; }
        public decimal CurrentRevenue { get; set; }
        public int CurrentViews { get; set; }
        public decimal AvgSearchPosition { get; set; }
        public bool ShouldOptimize { get; set; }
    }
}

