namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class LeftSidebarViewModel
    {
        public string CurrentCommunitySlug { get; set; } = string.Empty;
        public List<CommunityCardViewModel> JoinedCommunities { get; set; } = new();
        public List<CommunityCardViewModel> TrendingCommunities { get; set; } = new();
    }
}
