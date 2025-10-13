using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.HomePage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Services
{
    public class EnhancedHomeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EnhancedHomeService> _logger;

        public EnhancedHomeService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<EnhancedHomeService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<EnhancedHomePageViewModel> GetEnhancedHomePageDataAsync()
        {
            var model = new EnhancedHomePageViewModel
            {
                TrendingPosts = await GetTrendingPostsAsync(),
                FeaturedCommunities = await GetFeaturedCommunitiesAsync(),
                RecentPosts = await GetRecentPostsAsync(),
                Categories = await GetCategoriesAsync(),
                ActivityFeed = await GetLiveActivityFeedAsync(),
                SiteStats = await GetSiteStatsAsync(),
                TopContributors = await GetTopContributorsAsync(),
                TodayEarnings = await GetTodayEarningsAsync(),
                OnlineUsersCount = await GetOnlineUsersCountAsync()
            };

            return model;
        }

        private async Task<List<TrendingPostViewModel>> GetTrendingPostsAsync()
        {
            const string cacheKey = "enhanced_trending_posts";
            if (_cache.TryGetValue(cacheKey, out List<TrendingPostViewModel>? cached))
                return cached!;

            var last24Hours = DateTime.UtcNow.AddHours(-24);
            
            var trending = await _context.Posts
                .Where(p => p.Status == "published" && p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .Include(p => p.Community)
                .ThenInclude(c => c.Category)
                .Include(p => p.UserProfile)
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
                    CategoryName = p.Community.Category.Name,
                    AuthorDisplayName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                    IsHot = p.CommentCount > 20 || p.Score > 50
                })
                .ToListAsync();

            foreach (var post in trending)
            {
                var timeDiff = DateTime.UtcNow - _context.Posts.First(p => p.PostId == post.PostId).CreatedAt;
                post.TimeAgo = FormatTimeAgo(timeDiff);
            }

            _cache.Set(cacheKey, trending, TimeSpan.FromMinutes(5));
            return trending;
        }

        private async Task<List<FeaturedCommunityViewModel>> GetFeaturedCommunitiesAsync()
        {
            return await _context.Communities
                .Where(c => !c.IsDeleted)
                .Include(c => c.Category)
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
            return await _context.Posts
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
            return await _context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
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
            var recentActivities = await _context.UserActivities
                .Where(a => a.ActivityAt >= DateTime.UtcNow.AddMinutes(-30))
                .OrderByDescending(a => a.ActivityAt)
                .Take(10)
                .Select(a => new ActivityItemViewModel
                {
                    UserName = _context.UserProfiles.Where(u => u.UserId == a.UserId).Select(u => u.DisplayName).FirstOrDefault() ?? "Anonymous",
                    ActionType = a.ActivityType,
                    TargetTitle = a.PostId != null ? _context.Posts.Where(p => p.PostId == a.PostId).Select(p => p.Title).FirstOrDefault() ?? "" : "",
                    TimeAgo = FormatTimeAgo(DateTime.UtcNow - a.ActivityAt)
                })
                .ToListAsync();

            return new LiveActivityFeed
            {
                RecentActivities = recentActivities,
                TotalActivitiesLast24h = await _context.UserActivities
                    .Where(a => a.ActivityAt >= DateTime.UtcNow.AddHours(-24))
                    .CountAsync()
            };
        }

        private async Task<SiteStatsViewModel> GetSiteStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var fifteenMinsAgo = DateTime.UtcNow.AddMinutes(-15);

            return new SiteStatsViewModel
            {
                TotalMembers = await _context.UserProfiles.CountAsync(),
                TotalPosts = await _context.Posts.CountAsync(p => p.Status == "published"),
                TotalComments = await _context.Comments.CountAsync(c => !c.IsDeleted),
                OnlineNow = await _context.UserProfiles.CountAsync(u => u.LastActive > fifteenMinsAgo),
                PostsToday = await _context.Posts.CountAsync(p => p.CreatedAt >= today && p.Status == "published")
            };
        }

        private async Task<List<TopContributorViewModel>> GetTopContributorsAsync()
        {
            return await _context.UserProfiles
                .OrderByDescending(u => u.KarmaPoints)
                .Take(5)
                .Select(u => new TopContributorViewModel
                {
                    DisplayName = u.DisplayName,
                    Initials = GetInitials(u.DisplayName),
                    KarmaPoints = u.KarmaPoints,
                    PostsCount = _context.Posts.Count(p => p.UserId == u.UserId),
                    IsVerified = u.IsVerified
                })
                .ToListAsync();
        }

        private async Task<decimal> GetTodayEarningsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.AdSenseRevenues
                .Where(a => a.Date == today)
                .SumAsync(a => a.Earnings);
        }

        private async Task<int> GetOnlineUsersCountAsync()
        {
            var fifteenMinsAgo = DateTime.UtcNow.AddMinutes(-15);
            return await _context.UserProfiles.CountAsync(u => u.LastActive > fifteenMinsAgo);
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

