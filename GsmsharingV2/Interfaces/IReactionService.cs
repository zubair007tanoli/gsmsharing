using GsmsharingV2.DTOs;

namespace GsmsharingV2.Interfaces
{
    public interface IReactionService
    {
        Task<ReactionDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReactionDto>> GetByPostIdAsync(int postId);
        Task<IEnumerable<ReactionDto>> GetByCommentIdAsync(int commentId);
        Task<IEnumerable<ReactionSummaryDto>> GetReactionSummaryAsync(int? postId, int? commentId, string userId);
        Task<ReactionDto> ToggleReactionAsync(CreateReactionDto createReactionDto, string userId);
        Task<bool> DeleteReactionAsync(int id, string userId);
    }
}

