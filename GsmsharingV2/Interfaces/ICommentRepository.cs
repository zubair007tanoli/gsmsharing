using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
        Task<IEnumerable<Comment>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Comment>> GetRepliesAsync(int parentCommentId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        Task<int> GetCommentCountAsync(int postId);
    }
}

