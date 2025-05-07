using discussionspot.ViewModels;

namespace discussionspot.Interfaces
{
    public interface ICommentService
    {
        Task<CommentViewModel> GetCommentByIdAsync(int commentId);
        Task<IEnumerable<CommentViewModel>> GetPostCommentsAsync(int postId, string sortBy);
        Task<int> CreateCommentAsync(CommentCreateViewModel model, string userId);
        Task<bool> UpdateCommentAsync(int commentId, string content, string userId);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        Task<VoteResult> VoteCommentAsync(int commentId, string userId, bool isUpvote);
        Task<bool?> GetUserVoteForCommentAsync(int commentId, string userId);
        Task<IEnumerable<CommentViewModel>> GetRepliesAsync(int commentId);
        Task<IEnumerable<CommentViewModel>> GetUserCommentsAsync(string userId, int page, int pageSize);
    }
}
