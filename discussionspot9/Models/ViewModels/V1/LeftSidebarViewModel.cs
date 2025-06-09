using discussionspot9.Models.ViewModels.HomePage;

namespace discussionspot9.Models.ViewModels.V1
{
    public class LeftSidebarViewModel
    {
        public List<TrendingTopicViewModel> TrendingTopics { get; set; } = new();
        public DailyStatsViewModel DailyStats { get; set; } = new();
    }
}
