namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityListViewModel
    {
        public List<CommunityListItemViewModel> Communities { get; set; } = new();
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
        public string CurrentSort { get; set; } = "trending";
        public string CurrentCategory { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
