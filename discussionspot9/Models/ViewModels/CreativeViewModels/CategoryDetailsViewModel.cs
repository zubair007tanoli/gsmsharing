using discussionspot9.Models.Domain;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CategoryDetailsViewModel
    {
        public required Category Category { get; set; }
        public required List<Community> Communities { get; set; } = new();
        public required List<Category> RelatedCategories { get; set; } = new();
        public int TotalMembers { get; set; }
        public int TotalPosts { get; set; }
        public int WeeklyActivity { get; set; }
    }
}
