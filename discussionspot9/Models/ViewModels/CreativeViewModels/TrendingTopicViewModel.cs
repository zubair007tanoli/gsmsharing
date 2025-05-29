namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class TrendingTopicViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int PostCount { get; set; }
        public int TrendingScore { get; set; }
    }
}
