namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostListViewModel
    {
        public List<PostCardViewModel> Posts { get; set; } = new();
        public int TotalPosts { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CurrentSort { get; set; } = "hot";
        public string CurrentTimeFilter { get; set; } = "all";

        // For sidebar
        public List<TrendingCommunityViewModel> TrendingCommunities { get; set; } = new();
    }
}
