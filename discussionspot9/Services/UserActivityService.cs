using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    /// <summary>
    /// Service for tracking and retrieving user activities for admin dashboard
    /// </summary>
    public interface IUserActivityService
    {
        Task<List<UserActivity>> GetRecentActivitiesAsync(int limit = 50);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int days = 30);
        Task<Dictionary<string, int>> GetActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<UserActivitySummary>> GetTopActiveUsersAsync(int limit = 10, int days = 30);
    }

    public class UserActivityService : IUserActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserActivityService> _logger;

        public UserActivityService(
            ApplicationDbContext context,
            ILogger<UserActivityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<UserActivity>> GetRecentActivitiesAsync(int limit = 50)
        {
            try
            {
                return await _context.UserActivities
                    .Include(a => a.Post)
                    .Include(a => a.Community)
                    .OrderByDescending(a => a.ActivityAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities");
                return new List<UserActivity>();
            }
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                return await _context.UserActivities
                    .Where(a => a.UserId == userId && a.ActivityAt >= startDate)
                    .Include(a => a.Post)
                    .Include(a => a.Community)
                    .OrderByDescending(a => a.ActivityAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activities for {UserId}", userId);
                return new List<UserActivity>();
            }
        }

        public async Task<Dictionary<string, int>> GetActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.UserActivities.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(a => a.ActivityAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.ActivityAt <= endDate.Value);
                }

                var stats = await query
                    .GroupBy(a => a.ActivityType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type, x => x.Count);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity stats");
                return new Dictionary<string, int>();
            }
        }

        public async Task<List<UserActivitySummary>> GetTopActiveUsersAsync(int limit = 10, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                return await _context.UserActivities
                    .Where(a => a.UserId != null && a.ActivityAt >= startDate)
                    .GroupBy(a => a.UserId!)
                    .Select(g => new UserActivitySummary
                    {
                        UserId = g.Key,
                        ActivityCount = g.Count(),
                        LastActivity = g.Max(a => a.ActivityAt),
                        ActivityTypes = g.GroupBy(a => a.ActivityType)
                            .Select(ag => new { Type = ag.Key, Count = ag.Count() })
                            .ToDictionary(x => x.Type, x => x.Count)
                    })
                    .OrderByDescending(u => u.ActivityCount)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top active users");
                return new List<UserActivitySummary>();
            }
        }
    }

    public class UserActivitySummary
    {
        public string UserId { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
        public DateTime LastActivity { get; set; }
        public Dictionary<string, int> ActivityTypes { get; set; } = new();
    }
}

