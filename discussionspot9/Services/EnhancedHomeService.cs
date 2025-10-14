using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.HomePage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Services
{
    public class EnhancedHomeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EnhancedHomeService> _logger;

        public EnhancedHomeService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IMemoryCache cache,
            ILogger<EnhancedHomeService> logger)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<EnhancedHomePageViewModel> GetEnhancedHomePageDataAsync()
        {
            // Parallel execution - 5x faster
            var trendingPostsTask = GetTrendingPostsAsync();
            var featuredCommunitiesTask = GetFeaturedCommunitiesAsync();
            var recentPostsTask = GetRecentPostsAsync();
            var categoriesTask = GetCategoriesAsync();
            var activityFeedTask = GetLiveActivityFeedAsync();
            var siteStatsTask = GetSiteStatsAsync();
            var topContributorsTask = GetTopContributorsAsync();
            var todayEarningsTask = GetTodayEarningsAsync();
            var onlineUsersCountTask = GetOnlineUsersCountAsync();

            await Task.WhenAll(
                trendingPostsTask, featuredCommunitiesTask, recentPostsTask, categoriesTask,
                activityFeedTask, siteStatsTask, topContributorsTask, todayEarningsTask, onlineUsersCountTask
            );

            var model = new EnhancedHomePageViewModel
            {
                TrendingPosts = trendingPostsTask.Result,
                FeaturedCommunities = featuredCommunitiesTask.Result,
                RecentPosts = recentPostsTask.Result,
                Categories = categoriesTask.Result,
                ActivityFeed = activityFeedTask.Result,
                SiteStats = siteStatsTask.Result,
                TopContributors = topContributorsTask.Result,
                TodayEarnings = todayEarningsTask.Result,
                OnlineUsersCount = onlineUsersCountTask.Result
            };

            return model;
        }

        private async Task<List<TrendingPostViewModel>> GetTrendingPostsAsync()
        {
            const string cacheKey = "enhanced_trending_posts";
            if (_cache.TryGetValue(cacheKey, out List<TrendingPostViewModel>? cached))
                return cached!;

            await using var context = await _contextFactory.CreateDbContextAsync();
            var last24Hours = DateTime.UtcNow.AddHours(-24);
            
            var trending = await context.Posts
                .Where(p => p.Status == "published" && p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .Include(p => p.Community)
                .ThenInclude(c => c.Category)
                .Include(p => p.UserProfile)
                .AsNoTracking()
                .OrderByDescending(p => (p.Score * 3) + (p.CommentCount * 5) + (p.ViewCount / 10))
                .Take(6)
                .Select(p => new TrendingPostViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Slug = p.Slug,
                    CommunitySlug = p.Community.Slug,
                    CommunityName = p.Community.Name,
                    Score = p.Score,
                    CommentCount = p.CommentCount,
                    ViewCount = p.ViewCount,
                    CreatedAt = p.CreatedAt,
                    CategoryName = p.Community.Category.Name,
                    AuthorDisplayName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                    IsHot = p.CommentCount > 20 || p.Score > 50
                })
                .ToListAsync();

            foreach (var post in trending)
            {
                var timeDiff = DateTime.UtcNow - post.CreatedAt;
                post.TimeAgo = FormatTimeAgo(timeDiff);
            }

            _cache.Set(cacheKey, trending, TimeSpan.FromMinutes(5));
            return trending;
        }

        private async Task<List<FeaturedCommunityViewModel>> GetFeaturedCommunitiesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Communities
                .Where(c => !c.IsDeleted)
                .Include(c => c.Category)
                .AsNoTracking()
                .OrderByDescending(c => c.MemberCount)
                .Take(6)
                .Select(c => new FeaturedCommunityViewModel
                {
                    CommunityId = c.CommunityId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.ShortDescription ?? c.Description ?? "",
                    IconUrl = c.IconUrl ?? "",
                    MemberCount = c.MemberCount,
                    PostCount = c.PostCount,
                    CategoryName = c.Category != null ? c.Category.Name : "General",
                    IsGrowing = c.MemberCount > 100
                })
                .ToListAsync();
        }

        private async Task<List<RecentPostViewModel>> GetRecentPostsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Posts
                .Where(p => p.Status == "published")
                .Include(p => p.Community).ThenInclude(c => c.Category)
                .Include(p => p.UserProfile)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new RecentPostViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Slug = p.Slug,
                    Content = p.Content ?? "",
                    CategoryName = p.Community.Category.Name,
                    CategorySlug = p.Community.Category.Slug,
                    CommunitySlug = p.Community.Slug,
                    AuthorDisplayName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                    AuthorInitials = p.UserProfile != null ? GetInitials(p.UserProfile.DisplayName) : "?",
                    VoteCount = p.UpvoteCount - p.DownvoteCount,
                    CommentCount = p.CommentCount,
                    ViewCount = p.ViewCount,
                    CreatedAt = p.CreatedAt,
                    PostType = p.PostType,
                    IsPinned = p.IsPinned,
                    IsLocked = p.IsLocked,
                    Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList()
                })
                .ToListAsync();
        }

        private async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .AsNoTracking()
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description ?? "",
                    IconClass = GetCategoryIcon(c.Name),
                    TopicCount = c.Communities.Count,
                    PostCount = c.Communities.Sum(comm => comm.PostCount),
                    LastActivity = c.Communities.SelectMany(comm => comm.Posts)
                        .Where(p => p.Status == "published")
                        .Max(p => (DateTime?)p.CreatedAt)
                })
                .ToListAsync();
        }

        private async Task<LiveActivityFeed> GetLiveActivityFeedAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var recentActivities = await context.UserActivities
                .AsNoTracking()
                .Where(a => a.ActivityAt >= DateTime.UtcNow.AddMinutes(-30))
                .OrderByDescending(a => a.ActivityAt)
                .Take(10)
                .ToListAsync();
            
            // Bulk load related data to avoid N+1 queries
            var userIds = recentActivities.Where(a => !string.IsNullOrEmpty(a.UserId)).Select(a => a.UserId).Distinct().ToList();
            var postIds = recentActivities.Where(a => a.PostId.HasValue).Select(a => a.PostId!.Value).Distinct().ToList();
            
            var users = await context.UserProfiles
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.DisplayName);
            
            var posts = await context.Posts
                .AsNoTracking()
                .Where(p => postIds.Contains(p.PostId))
                .ToDictionaryAsync(p => p.PostId, p => p.Title);
            
            var activityViewModels = recentActivities.Select(a => new ActivityItemViewModel
            {
                UserName = !string.IsNullOrEmpty(a.UserId) && users.ContainsKey(a.UserId) 
                    ? users[a.UserId] 
                    : "Anonymous",
                ActionType = a.ActivityType,
                TargetTitle = a.PostId.HasValue && posts.ContainsKey(a.PostId.Value) 
                    ? posts[a.PostId.Value] 
                    : "",
                TimeAgo = FormatTimeAgo(DateTime.UtcNow - a.ActivityAt)
            }).ToList();

            return new LiveActivityFeed
            {
                RecentActivities = activityViewModels,
                TotalActivitiesLast24h = await context.UserActivities
                    .AsNoTracking()
                    .Where(a => a.ActivityAt >= DateTime.UtcNow.AddHours(-24))
                    .CountAsync()
            };
        }

        private async Task<SiteStatsViewModel> GetSiteStatsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.UtcNow.Date;
            var fifteenMinsAgo = DateTime.UtcNow.AddMinutes(-15);

            return new SiteStatsViewModel
            {
                TotalMembers = await context.UserProfiles.AsNoTracking().CountAsync(),
                TotalPosts = await context.Posts.AsNoTracking().CountAsync(p => p.Status == "published"),
                TotalComments = await context.Comments.AsNoTracking().CountAsync(c => !c.IsDeleted),
                OnlineNow = await context.UserProfiles.AsNoTracking().CountAsync(u => u.LastActive > fifteenMinsAgo),
                PostsToday = await context.Posts.AsNoTracking().CountAsync(p => p.CreatedAt >= today && p.Status == "published")
            };
        }

        private async Task<List<TopContributorViewModel>> GetTopContributorsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var topUsers = await context.UserProfiles
                .AsNoTracking()
                .OrderByDescending(u => u.KarmaPoints)
                .Take(5)
                .Select(u => new 
                {
                    u.UserId,
                    u.DisplayName,
                    u.KarmaPoints,
                    u.IsVerified
                })
                .ToListAsync();

            // Bulk load post counts to avoid N+1
            var userIds = topUsers.Select(u => u.UserId).ToList();
            var postCounts = await context.Posts
                .AsNoTracking()
                .Where(p => userIds.Contains(p.UserId))
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            return topUsers.Select(u => new TopContributorViewModel
            {
                DisplayName = u.DisplayName,
                Initials = GetInitials(u.DisplayName),
                KarmaPoints = u.KarmaPoints,
                PostsCount = postCounts.ContainsKey(u.UserId) ? postCounts[u.UserId] : 0,
                IsVerified = u.IsVerified
            }).ToList();
        }

        private async Task<decimal> GetTodayEarningsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.UtcNow.Date;
            return await context.AdSenseRevenues
                .AsNoTracking()
                .Where(a => a.Date == today)
                .SumAsync(a => a.Earnings);
        }

        private async Task<int> GetOnlineUsersCountAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var fifteenMinsAgo = DateTime.UtcNow.AddMinutes(-15);
            return await context.UserProfiles.AsNoTracking().CountAsync(u => u.LastActive > fifteenMinsAgo);
        }

        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return "?";

            var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            
            return displayName.Length >= 2 ? displayName.Substring(0, 2).ToUpper() : displayName.ToUpper();
        }

        private static string GetCategoryIcon(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "technology" => "fas fa-laptop-code",
                "gaming" => "fas fa-gamepad",
                "science" => "fas fa-flask",
                "programming" => "fas fa-code",
                "business" => "fas fa-briefcase",
                _ => "fas fa-folder"
            };
        }

        private static string FormatTimeAgo(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
        }
    }
}

