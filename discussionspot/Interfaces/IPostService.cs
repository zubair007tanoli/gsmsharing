using discussionspot.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IPostService
    {// Post retrieval methods
        Task<IEnumerable<PostViewModel>> GetPostsAsync(string sortBy, string timeFilter, int? communityId);
        Task<PostViewModel> GetPostByIdAsync(int id);
        Task<PostCreateViewModel> GetPostForEditingAsync(int postId);
        Task<IEnumerable<PostViewModel>> GetRelatedPostsAsync(int postId, int count);

        // Filtered post queries (from IPostTasks)
        Task<List<PostViewModel>> GetPostsByUserIdAsync(string userId);
        Task<List<PostViewModel>> GetPostsByCommunityIdAsync(int communityId);
        Task<List<PostViewModel>> GetPostsByCategoryIdAsync(int categoryId);
        Task<List<PostViewModel>> GetPostsBySearchTermAsync(string searchTerm);
        Task<List<PostViewModel>> GetPostsByStatusAsync(string status);
        Task<List<PostViewModel>> GetPostsByTypeAsync(string type);
        Task<List<PostViewModel>> GetPostsByPaginationAsync(int pageNumber, int pageSize);

        // Post comment methods
        Task<IEnumerable<CommentViewModel>> GetCommentsForPostAsync(int postId);

        // Post manipulation methods
        Task<int> CreatePostAsync(PostCreateViewModel model, string userId);
        Task<bool> UpdatePostAsync(int postId, PostCreateViewModel model, string userId);
        Task<bool> DeletePostAsync(int postId);

        // Post voting and interaction
        Task<bool?> GetUserVoteForPostAsync(int postId, string userId);
        Task<VoteResult> VotePostAsync(int postId, string userId, bool isUpvote);

        // Draft functionality
        Task<int> SaveDraftAsync(PostCreateViewModel model, string userId);
        Task<IEnumerable<PostDraftViewModel>> GetDraftsAsync(string userId);

        // Conversion methods
        PostCreateViewModel CreateEditViewModel(PostViewModel post);
    }
}
