using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface ICommentService
    {
        Task<CreateCommentResult> CreateCommentAsync(CreateCommentViewModel model);
        Task<CommentViewModel?> GetCommentByIdAsync(int commentId);
        Task<VoteResult> VoteCommentAsync(int commentId, string userId, int voteType);
        Task<int> GetCommentVoteCountAsync(int commentId);
        Task<ServiceResult> EditCommentAsync(int commentId, string content, string userId);
        Task<ServiceResult> DeleteCommentAsync(int commentId, string userId);
        Task<List<CommentTreeViewModel>> GetPostCommentsAsync(int postId, string sort = "best", int page = 1);
        Task UpdateCommentAsync(int commentId, string newContent, string? userId);
    }
}
