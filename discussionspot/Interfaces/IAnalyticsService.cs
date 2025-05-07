using discussionspot.Models;

namespace discussionspot.Interfaces
{
    public interface IAnalyticsService
    {
        Task TrackPageViewAsync(string path, string? userId = null);
        Task TrackPostViewAsync(int postId, string? userId = null);
        Task TrackSearchQueryAsync(string query, string? userId = null);
        Task<IEnumerable<AnalyticsDataPoint>> GetPageViewsAsync(DateTime start, DateTime end);
        Task<IEnumerable<AnalyticsDataPoint>> GetPostViewsByDayAsync(int postId, int days);
        Task<IEnumerable<AnalyticsDataPoint>> GetCommunityViewsByDayAsync(int communityId, int days);
        Task<IEnumerable<AnalyticsDataPoint>> GetPopularSearchTermsAsync(int count);
        Task<IEnumerable<AnalyticsDataPoint>> GetUserActivityAsync(string userId, int days);
    }
}
