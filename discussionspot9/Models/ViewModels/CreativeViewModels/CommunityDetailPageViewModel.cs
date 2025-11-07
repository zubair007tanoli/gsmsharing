namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityDetailPageViewModel
    {
        public CommunityDetailViewModel Community { get; set; } = new CommunityDetailViewModel 
        { 
            ThemeColor = "#667eea", 
            ShortDescription = string.Empty 
        };
        public PostListViewModel Posts { get; set; } = new();
        public string CurrentSort { get; set; } = "hot";
        public int CurrentPage { get; set; } = 1;
    }
}
