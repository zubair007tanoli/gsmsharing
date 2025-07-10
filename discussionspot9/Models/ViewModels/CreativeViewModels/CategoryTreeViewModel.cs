using System.Collections.Generic;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CategoryTreeViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; 
 
        public List<CommunityViewModel> Communities { get; set; } = new(); 
        public List<CategoryTreeViewModel> ChildCategories { get; set; } = new();
    }
}
