using discussionspot9.Models.Domain;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CategoryDetailsViewModel
    {
        public Category Category { get; set; }
        public List<Community> Communities { get; set; }
        public List<Category> RelatedCategories { get; set; }
        public int TotalMembers { get; set; }
        public int TotalPosts { get; set; }
        public int WeeklyActivity { get; set; }
    }
}
