using discussionspot9.Models.ViewModels.CreativeViewModels;

namespace discussionspot9.Models.ViewModels.V1
{
    public class CommentSectionViewModel
    {
        public List<CommentTreeViewModel> Comments { get; set; } = new();
        public int PostId { get; set; }
        public int TotalCount { get; set; }
    }
}
