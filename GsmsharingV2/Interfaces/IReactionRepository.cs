using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface IReactionRepository
    {
        Task<Reaction?> GetByIdAsync(int id);
        Task<Reaction?> GetByUserAndPostAsync(string userId, int postId);
        Task<Reaction?> GetByUserAndCommentAsync(string userId, int commentId);
        Task<IEnumerable<Reaction>> GetByPostIdAsync(int postId);
        Task<IEnumerable<Reaction>> GetByCommentIdAsync(int commentId);
        Task<Reaction> CreateAsync(Reaction reaction);
        Task DeleteAsync(int id);
        Task<int> GetReactionCountAsync(int? postId, int? commentId, string reactionType);
        Task<bool> UserHasReactedAsync(string userId, int? postId, int? commentId, string reactionType);
    }
}

