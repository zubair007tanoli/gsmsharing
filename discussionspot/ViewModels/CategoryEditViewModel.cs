namespace discussionspot.ViewModels
{
    public class CategoryEditViewModel: CategoryCreateViewModel
    {
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
