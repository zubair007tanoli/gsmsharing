using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface IReactionRepository
    {
        Task<Reaction> GetByIdAsync(int id);
        Task<IEnumerable<Reaction>> GetAllAsync();
        Task<IEnumerable<Reaction>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Reaction>> GetByPostIdAsync(int postId);
        Task<IEnumerable<Reaction>> GetByCommentIdAsync(int commentId);
        Task<IEnumerable<Reaction>> GetByReactionTypeAsync(string reactionType);
        Task<Reaction> CreateAsync(Reaction reaction);
        Task<Reaction> UpdateAsync(Reaction reaction);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Reaction>> GetPaginatedAsync(int page, int pageSize);
        Task<int> GetReactionCountForPostAsync(int postId);
        Task<int> GetReactionCountForCommentAsync(int commentId);
        Task<bool> HasUserReactedToPostAsync(string userId, int postId);
        Task<bool> HasUserReactedToCommentAsync(string userId, int commentId);
    }
}
