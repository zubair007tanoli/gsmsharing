using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ICommentAwardRepository : IRepository<CommentAward>
    {
        Task<IEnumerable<CommentAward>> GetAwardsForCommentAsync(int commentId);
        Task<IEnumerable<CommentAward>> GetCommentAwardsGivenByUserAsync(string userId);
        Task<IEnumerable<CommentAward>> GetCommentAwardsReceivedByUserAsync(string userId);
    }
}
