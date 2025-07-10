using System.Collections.Generic;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CategoryCommunitiesViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Slug { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public List<CommunityViewModel> Communities { get; set; } = new(); 
        public int TotalCommunities { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<string> TrendingTopics { get; set; } = new();
    }
}
