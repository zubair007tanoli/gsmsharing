using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IPostService
    {
        Task<PostViewModel> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostViewModel>> GetPostsAsync(string sortBy = "hot", int page = 1, int pageSize = 20);
        Task<IEnumerable<PostViewModel>> GetPostsByCommunityAsync(int communityId, int page = 1, int pageSize = 20);
        Task<int> CreatePostAsync(PostCreateViewModel model, string userId);
        Task<bool> UpdatePostAsync(int postId, PostEditViewModel model, string userId);
        Task<bool> DeletePostAsync(int postId, string userId);
        Task<VoteResult> VotePostAsync(int postId, string userId, bool isUpvote);
        Task<bool?> GetUserVoteForPostAsync(int postId, string userId);
        Task IncrementViewCountAsync(int postId);
    }
}
