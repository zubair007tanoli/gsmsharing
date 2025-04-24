namespace discussionspot.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCommunities { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int NewUsersToday { get; set; }
        public int NewPostsToday { get; set; }
        public int ActiveUsers24Hours { get; set; }
        public List<PostListItemViewModel> TrendingPosts { get; set; }
        public List<CommunityViewModel> TrendingCommunities { get; set; }
    }
}
