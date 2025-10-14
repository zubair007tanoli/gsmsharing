// Services/IHomeService.cs
using discussionspot9.Models.ViewModels.HomePage;
using Microsoft.Extensions.Caching.Memory;
// Services/HomeService.cs
using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using discussionspot9.Interfaces;

namespace discussionspot9.Services
{
    public class HomeService : IHomeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HomeService> _logger;

        public HomeService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IMemoryCache cache,
            ILogger<HomeService> logger)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<HomePageViewModel> GetHomePageDataAsync()
        {
            try
            {
                // Parallel execution - load all data simultaneously
                var randomPostsTask = GetRandomPostsAsync();
                var categoriesTask = GetCategoriesAsync();
                var recentPostsTask = GetRecentPostsAsync();
                var sidebarTask = GetSidebarDataAsync();

                await Task.WhenAll(randomPostsTask, categoriesTask, recentPostsTask, sidebarTask);

                return new HomePageViewModel
                {
                    RandomPosts = randomPostsTask.Result,
                    Categories = categoriesTask.Result,
                    RecentPosts = recentPostsTask.Result,
                    Sidebar = sidebarTask.Result
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching homepage data");
                return new HomePageViewModel();
            }
        }

        public async Task<List<RandomPostViewModel>> GetRandomPostsAsync(int count = 4)
        {
            const string cacheKey = "homepage_random_posts";

            if (_cache.TryGetValue(cacheKey, out List<RandomPostViewModel>? cachedPosts))
                return cachedPosts!;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var randomPosts = await context.Posts
                    .Where(p => p.Status == "published")
                    .Include(p => p.Community)
                    .ThenInclude(c => c!.Category)
                    .Include(p => p.UserProfile)
                    .AsNoTracking()
                    .OrderBy(r => Guid.NewGuid())
                    .Take(count)
                    .Select(p => new RandomPostViewModel
                    {
                        PostId = p.PostId,
                        Title = p.Title,
                        Slug = p.Slug,
                        CommunityName = p.Community!.Name,
                        CommunitySlug = p.Community.Slug,
                        CategoryName = p.Community.Category!.Name,
                        CategorySlug = p.Community.Category.Slug,
                        AuthorDisplayName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                        CommentCount = p.CommentCount,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                foreach (var post in randomPosts)
                {
                    post.AuthorInitials = GetInitials(post.AuthorDisplayName);
                }

                _cache.Set(cacheKey, randomPosts, TimeSpan.FromMinutes(10));
                return randomPosts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching random posts");
                return new List<RandomPostViewModel>();
            }
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            const string cacheKey = "homepage_categories";

            if (_cache.TryGetValue(cacheKey, out List<CategoryViewModel>? cachedCategories))
                return cachedCategories!;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var categories = await context.Categories
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
                        LastActivity = c.Communities
                            .SelectMany(comm => comm.Posts)
                            .Where(p => p.Status == "published")
                            .Max(p => (DateTime?)p.CreatedAt)
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                foreach (var category in categories)
                {
                    category.IsActiveNow = category.LastActivity.HasValue &&
                                         category.LastActivity.Value > oneHourAgo;
                }

                _cache.Set(cacheKey, categories, TimeSpan.FromHours(1));
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return new List<CategoryViewModel>();
            }
        }

        public async Task<List<RecentPostViewModel>> GetRecentPostsAsync(int count = 3)
        {
            const string cacheKey = "homepage_recent_posts";

            if (_cache.TryGetValue(cacheKey, out List<RecentPostViewModel>? cachedPosts))
                return cachedPosts!;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var recentPosts = await context.Posts
                    .Where(p => p.Status == "published")
                    .Include(p => p.Community)
                    .ThenInclude(c => c!.Category)
                    .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                    .Include(p => p.UserProfile)
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .Select(p => new RecentPostViewModel
                    {
                        PostId = p.PostId,
                        Title = p.Title,
                        Slug = p.Slug,
                        Content = p.Content ?? "",
                        CategoryName = p.Community!.Category!.Name,
                        CategorySlug = p.Community.Category.Slug,
                        CommunitySlug = p.Community.Slug,
                        AuthorDisplayName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                        VoteCount = p.UpvoteCount - p.DownvoteCount,
                        CommentCount = p.CommentCount,
                        ViewCount = p.ViewCount,
                        CreatedAt = p.CreatedAt,
                        Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList()
                    })
                    .ToListAsync();

                foreach (var post in recentPosts)
                {
                    post.Excerpt = GenerateExcerpt(post.Content, 150);
                    post.AuthorInitials = GetInitials(post.AuthorDisplayName);
                }

                _cache.Set(cacheKey, recentPosts, TimeSpan.FromMinutes(5));
                return recentPosts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent posts");
                return new List<RecentPostViewModel>();
            }
        }

        public async Task<SidebarViewModel> GetSidebarDataAsync()
        {
            try
            {
                var trendingTask = await GetTrendingTopicsAsync();
                var onlineUsersTask = await GetOnlineUsersDataAsync();
                var statsTask = await GetForumStatsAsync();
                var newMembersTask =await  GetNewMembersAsync();                
                return new SidebarViewModel
                {
                    TrendingTopics = trendingTask,
                    OnlineUsers =  onlineUsersTask,
                    ForumStats =  statsTask,
                    NewMembers =  newMembersTask
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sidebar data");
                return new SidebarViewModel();
            }
        }

        private async Task<List<TrendingTopicViewModel>> GetTrendingTopicsAsync()
        {
            const string cacheKey = "homepage_trending_topics";
            if (_cache.TryGetValue(cacheKey, out List<TrendingTopicViewModel>? cached))
                return cached!;

            await using var context = await _contextFactory.CreateDbContextAsync();
            var trending = await context.Posts
                .Where(p => p.Status == "published") // Remove date filter for now
                .Include(p => p.Community)
                .ThenInclude(c => c!.Category)
                .AsNoTracking()
                .OrderByDescending(p => (p.Score * 2) + (p.CommentCount * 3) + p.ViewCount) // Better trending algorithm
                .Take(5) // Show 5 instead of 4
                .Select(p => new TrendingTopicViewModel
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Slug = p.Slug,
                    CategoryName = p.Community!.Category!.Name,
                    CategorySlug = p.Community.Category.Slug,
                    ReplyCount = p.CommentCount,
                    IsHot = p.CommentCount > 5 || p.Score > 50, // Better hot logic
                    CreatedAt = p.CreatedAt,
                    LastActivity = p.UpdatedAt
                })
                .ToListAsync();

            _cache.Set(cacheKey, trending, TimeSpan.FromMinutes(10)); // Shorter cache
            return trending;
        }

        private async Task<OnlineUsersViewModel> GetOnlineUsersDataAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);

            // Get actual online users
            var onlineUsers = await context.UserProfiles
                .Where(u => u.LastActive > fifteenMinutesAgo)
                .AsNoTracking()
                .OrderByDescending(u => u.LastActive)
                .Take(10)
                .Select(u => new OnlineUserViewModel
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    Initials = u.DisplayName.Length >= 2 ?
                        u.DisplayName.Substring(0, 2).ToUpper() :
                        u.DisplayName.ToUpper(),                 
                    LastActivity = u.LastActive
                })
                .ToListAsync();

            // Get category activity
            var categoryActivity = await context.Posts
                .Where(p => p.CreatedAt > DateTime.UtcNow.AddHours(-24))
                .AsNoTracking()
                .GroupBy(p => p.Community.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            return new OnlineUsersViewModel
            {
                TotalOnline = onlineUsers.Count,
                PeakToday = onlineUsers.Count + new Random().Next(5, 15), // Simulated peak
                PeakTime = DateTime.Today.AddHours(14),
                VisibleUsers = onlineUsers,
                ActiveInCategories = categoryActivity
            };
        }

        private async Task<ForumStatsViewModel> GetForumStatsAsync()
        {
            const string cacheKey = "homepage_forum_stats";

            if (_cache.TryGetValue(cacheKey, out ForumStatsViewModel? cached))
                return cached!;

            await using var context = await _contextFactory.CreateDbContextAsync();
            var stats = new ForumStatsViewModel
            {
                TotalMembers = await context.UserProfiles.AsNoTracking().CountAsync(),
                TotalPosts = await context.Posts.AsNoTracking().CountAsync(p => p.Status == "published"),
                TotalCategories = await context.Categories.AsNoTracking().CountAsync(c => c.IsActive),
                LastPostTime = await context.Posts
                    .AsNoTracking()
                    .Where(p => p.Status == "published")
                    .MaxAsync(p => (DateTime?)p.CreatedAt) ?? DateTime.UtcNow
            };

            stats.LastPostTimeAgo = GetTimeAgo(stats.LastPostTime);

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(15));
            return stats;
        }

        private async Task<List<NewMemberViewModel>> GetNewMembersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var newMembers = await context.UserProfiles
                .OrderByDescending(u => u.JoinDate)
                .Take(3)
                .Select(u => new NewMemberViewModel
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    JoinDate = u.JoinDate
                })
                .ToListAsync();

            foreach (var member in newMembers)
            {
                member.Initials = GetInitials(member.DisplayName);
                member.JoinDateAgo = GetTimeAgo(member.JoinDate);
            }

            return newMembers;
        }

        // Helper methods
        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return "??";

            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }

        private static string GetCategoryIcon(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "general" or "general discussion" => "fas fa-comments",
                "technology" or "tech" => "fas fa-laptop",
                "gaming" or "games" => "fas fa-gamepad",
                "creative arts" or "art" => "fas fa-palette",
                _ => "fas fa-folder"
            };
        }

        private static string GenerateExcerpt(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            if (content.Length <= maxLength)
                return content;

            var excerpt = content.Substring(0, maxLength);
            var lastSpace = excerpt.LastIndexOf(' ');

            if (lastSpace > 0)
                excerpt = excerpt.Substring(0, lastSpace);

            return excerpt + "...";
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)timeSpan.TotalMinutes} minutes ago",
                < 86400 => $"{(int)timeSpan.TotalHours} hours ago",
                < 2592000 => $"{(int)timeSpan.TotalDays} days ago",
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }
    }
}