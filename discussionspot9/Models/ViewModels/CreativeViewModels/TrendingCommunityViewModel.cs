namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class TrendingCommunityViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int GrowthPercentage { get; set; }
    }
}
