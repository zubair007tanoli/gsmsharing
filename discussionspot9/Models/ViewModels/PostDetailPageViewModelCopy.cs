using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.PollViewModels;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostDetailPageViewModelCopy
    {
        public PostDetailViewModel Post { get; set; }
        public Category category { get; set; } // Added this property
        public List<CommentTreeViewModel> Comments { get; set; }
        public string CommunitySlug { get; set; }
        public string PostSlug { get; set; }
        public CommunityDetailViewModel? Community { get; set; }
        public string CategorySlug { get; set; } // Added this property
        public List<PostCardViewModel> RelatedPosts { get; set; }
    }
}