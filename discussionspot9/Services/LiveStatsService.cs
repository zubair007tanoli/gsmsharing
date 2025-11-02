using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.HomePage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Services
{
    public class LiveStatsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<LiveStatsService> _logger;

        public LiveStatsService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IMemoryCache cache,
            IPresenceService presenceService,
            ILogger<LiveStatsService> logger)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _presenceService = presenceService;
            _logger = logger;
        }

        public async Task<LiveStatsViewModel> GetLiveStatsAsync()
        {
            const string cacheKey = "live_stats_header";
            
            // Cache for 2 minutes for performance
            if (_cache.TryGetValue(cacheKey, out LiveStatsViewModel? cached))
                return cached!;

            try
            {
                // Parallel execution for better performance
                var onlineUsersTask = GetOnlineUserCountAsync();
                var trendingPostTask = GetTrendingPostAsync();
                var postsLastHourTask = GetPostsLastHourAsync();
                var topContributorTask = GetTopContributorAsync();
                var hotTopicTask = GetHotTopicAsync();

                await Task.WhenAll(onlineUsersTask, trendingPostTask, postsLastHourTask, topContributorTask, hotTopicTask);

                var stats = new LiveStatsViewModel
                {
                    OnlineUsersCount = await onlineUsersTask,
                    TrendingPost = await trendingPostTask,
                    PostsLastHour = await postsLastHourTask,
                    TopContributor = await topContributorTask,
                    HotTopic = await hotTopicTask
                };

                _cache.Set(cacheKey, stats, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                    Priority = CacheItemPriority.Normal
                });

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching live stats");
                // Return sensible defaults on error
                return new LiveStatsViewModel
                {
                    OnlineUsersCount = 0,
                    TrendingPost = "No trending post available",
                    PostsLastHour = 0,
                    TopContributor = "No data available",
                    HotTopic = new HotTopicViewModel { Title = "No hot topics", ViewCount = 0 }
                };
            }
        }

        private async Task<int> GetOnlineUserCountAsync()
        {
            try
            {
                // Use UserPresences table for real-time data
                await using var context = await _contextFactory.CreateDbContextAsync();
                var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
                
                var count = await context.UserPresences
                    .Where(p => p.LastSeen > fifteenMinutesAgo)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online user count");
                // Fallback to UserProfiles with LastActive
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
                    return await context.UserProfiles
                        .CountAsync(u => u.LastActive > fifteenMinutesAgo);
                }
                catch
                {
                    return 0;
                }
            }
        }

        private async Task<string> GetTrendingPostAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var last24Hours = DateTime.UtcNow.AddHours(-24);

                var trendingPost = await context.Posts
                    .Where(p => p.Status == "published" && p.CreatedAt >= last24Hours)
                    .OrderByDescending(p => (p.Score * 3) + (p.CommentCount * 5) + (p.ViewCount / 10))
                    .FirstOrDefaultAsync();

                if (trendingPost != null)
                {
                    var title = trendingPost.Title.Length > 50 
                        ? trendingPost.Title.Substring(0, 50) + "..." 
                        : trendingPost.Title;
                    return $"\"{title}\" - {trendingPost.CommentCount} comments";
                }

                return "No trending posts yet";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending post");
                return "No trending posts yet";
            }
        }

        private async Task<int> GetPostsLastHourAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                return await context.Posts
                    .CountAsync(p => p.Status == "published" && p.CreatedAt >= oneHourAgo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts last hour");
                return 0;
            }
        }

        private async Task<string> GetTopContributorAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

                var topContributor = await context.Posts
                    .Where(p => p.Status == "published" && p.CreatedAt >= oneWeekAgo)
                    .GroupBy(p => p.UserProfile.DisplayName)
                    .Select(g => new { DisplayName = g.Key, PostCount = g.Count() })
                    .OrderByDescending(x => x.PostCount)
                    .FirstOrDefaultAsync();

                if (topContributor != null)
                {
                    return $"@{topContributor.DisplayName} - {topContributor.PostCount} posts this week";
                }

                return "No top contributor yet";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top contributor");
                return "No top contributor yet";
            }
        }

        private async Task<HotTopicViewModel> GetHotTopicAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var last24Hours = DateTime.UtcNow.AddHours(-24);

                var hotTopic = await context.Posts
                    .Where(p => p.Status == "published" && p.CreatedAt >= last24Hours)
                    .OrderByDescending(p => p.ViewCount)
                    .FirstOrDefaultAsync();

                if (hotTopic != null)
                {
                    return new HotTopicViewModel
                    {
                        Title = hotTopic.Title.Length > 40 
                            ? hotTopic.Title.Substring(0, 40) + "..." 
                            : hotTopic.Title,
                        ViewCount = hotTopic.ViewCount
                    };
                }

                return new HotTopicViewModel { Title = "No hot topics", ViewCount = 0 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hot topic");
                return new HotTopicViewModel { Title = "No hot topics", ViewCount = 0 };
            }
        }
    }

    public class LiveStatsViewModel
    {
        public int OnlineUsersCount { get; set; }
        public string TrendingPost { get; set; } = "";
        public int PostsLastHour { get; set; }
        public string TopContributor { get; set; } = "";
        public HotTopicViewModel HotTopic { get; set; } = new();
    }

    public class HotTopicViewModel
    {
        public string Title { get; set; } = "";
        public int ViewCount { get; set; }
        
        public string FormattedViewCount => ViewCount switch
        {
            >= 1000000 => $"{ViewCount / 1000000.0:F1}M",
            >= 1000 => $"{ViewCount / 1000.0:F1}K",
            _ => ViewCount.ToString()
        };
    }
}

