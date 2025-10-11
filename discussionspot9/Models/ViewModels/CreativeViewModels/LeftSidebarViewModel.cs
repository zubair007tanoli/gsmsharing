namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class LeftSidebarViewModel
    {
        public string? CurrentCommunitySlug { get; set; }
        public List<NewsItemViewModel> LatestNews { get; set; } = new List<NewsItemViewModel>();
        public TodayStatsViewModel TodayStats { get; set; } = new TodayStatsViewModel();
        public List<CommunityCardViewModel> JoinedCommunities { get; set; } = new List<CommunityCardViewModel>();
        public List<CommunityCardViewModel> TrendingCommunities { get; set; } = new List<CommunityCardViewModel>();
    }

    public class NewsItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string TimeAgo { get; set; } = string.Empty;
        
        public string CategoryClass
        {
            get
            {
                return Category.ToLower() switch
                {
                    "technology" or "tech" => "tech",
                    "gaming" => "gaming",
                    "programming" => "programming",
                    "science" => "science",
                    _ => "general"
                };
            }
        }
    }

    public class TodayStatsViewModel
    {
        public int NewPostsCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int CommentsCount { get; set; }

        public string FormattedNewPosts => FormatCount(NewPostsCount);
        public string FormattedActiveUsers => FormatCount(ActiveUsersCount);
        public string FormattedComments => FormatCount(CommentsCount);

        private string FormatCount(int count)
        {
            if (count >= 1000000)
                return $"{count / 1000000.0:0.#}M";
            if (count >= 1000)
                return $"{count / 1000.0:0.#}K";
            return count.ToString("#,##0");
        }
    }
}
