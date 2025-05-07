namespace discussionspot.Interfaces
{
    public interface IStatisticsService
    {
        Task<int> GetTotalPostCountAsync();
        Task<int> GetTotalCommentCountAsync();
        Task<int> GetTotalUserCountAsync();
        Task<int> GetTotalCommunityCountAsync();
        Task<int> GetActiveUserCountAsync(int minutes);
        Task<int> GetPostsInLastDaysAsync(int days);
        Task<int> GetCommentsInLastDaysAsync(int days);
        Task<int> GetNewUsersInLastDaysAsync(int days);
        Task<Dictionary<string, int>> GetPostTypeDistributionAsync();
        Task<Dictionary<DateTime, int>> GetPostsPerDayAsync(int days);
    }
}
