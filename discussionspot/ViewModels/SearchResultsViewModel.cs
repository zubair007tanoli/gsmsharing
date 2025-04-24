namespace discussionspot.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; }
        public string SearchType { get; set; }
        public string SortBy { get; set; }
        public string TimeFrame { get; set; }
        public int TotalResults { get; set; }

        public PaginatedList<PostListItemViewModel> Posts { get; set; }
        public PaginatedList<CommunityViewModel> Communities { get; set; }
        public PaginatedList<UserProfileViewModel> Users { get; set; }
    }
}
