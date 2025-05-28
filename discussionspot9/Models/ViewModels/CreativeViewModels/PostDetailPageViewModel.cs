namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostDetailPageViewModel
    {
        public PostDetailViewModel Post { get; set; } = new();
        public List<CommentTreeViewModel> Comments { get; set; } = new();
        public string CommunitySlug { get; set; } = string.Empty;
        public string PostSlug { get; set; } = string.Empty;
        public CommunityDetailViewModel? Community { get; set; }
        public List<PostCardViewModel> RelatedPosts { get; set; } = new();
    }
}
