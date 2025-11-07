using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.PollViewModels;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostDetailPageViewModelCopy
    {
        public required PostDetailViewModel Post { get; set; }
        public Category? category { get; set; } // Added this property
        public required List<CommentTreeViewModel> Comments { get; set; } = new();
        public required string CommunitySlug { get; set; }
        public required string PostSlug { get; set; }
        public CommunityDetailViewModel? Community { get; set; }
        public string CategorySlug { get; set; } = string.Empty; // Added this property
        public List<PostCardViewModel> RelatedPosts { get; set; } = new();     

    }
}
