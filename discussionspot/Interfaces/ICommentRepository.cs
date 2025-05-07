using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<Comment?> GetCommentWithDetailsAsync(int commentId);
        Task<IEnumerable<Comment>> GetPostCommentsAsync(int postId);
        Task<IEnumerable<Comment>> GetUserCommentsAsync(string userId, int page, int pageSize);
        Task<IEnumerable<Comment>> GetTopLevelCommentsAsync(int postId);
        Task<IEnumerable<Comment>> GetCommentRepliesAsync(int commentId);
        Task<IEnumerable<Comment>> GetCommentTreeAsync(int postId);
        Task UpdateCommentScoreAsync(int commentId);
    }
}
