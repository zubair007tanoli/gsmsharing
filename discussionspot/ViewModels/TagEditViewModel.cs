namespace discussionspot.ViewModels
{
    public class TagEditViewModel : TagCreateViewModel
    {
        public int TagId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PostCount { get; set; }
    }
}
