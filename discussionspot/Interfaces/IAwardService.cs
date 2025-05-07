using discussionspot.Models.Domain;

namespace discussionspot.Interfaces
{
    public interface IAwardService
    {
        Task<IEnumerable<Award>> GetAvailableAwardsAsync();
        Task<bool> GivePostAwardAsync(int postId, int awardId, string userId, string? message = null, bool isAnonymous = false);
        Task<bool> GiveCommentAwardAsync(int commentId, int awardId, string userId, string? message = null, bool isAnonymous = false);
        Task<IEnumerable<PostAward>> GetAwardsForPostAsync(int postId);
        Task<IEnumerable<CommentAward>> GetAwardsForCommentAsync(int commentId);
    }
}
