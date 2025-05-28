namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityListViewModel
    {
        public List<CommunityCardViewModel> Communities { get; set; } = new();
        public int TotalCommunities { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CurrentSort { get; set; } = "popular";
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
    }
}
