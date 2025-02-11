using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
        Task<IEnumerable<Comment>> GetByUserIdAsync(string userId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Comment>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<Comment>> GetByParentCommentIdAsync(int parentCommentId);
    }
}
