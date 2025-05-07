using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IPostVoteRepository : IRepository<PostVote>
    {
        Task<PostVote?> GetUserVoteForPostAsync(string userId, int postId);
        Task<IEnumerable<PostVote>> GetVotesForPostAsync(int postId);
        Task<IEnumerable<PostVote>> GetUserVotesAsync(string userId);
        Task<int> GetUpvoteCountAsync(int postId);
        Task<int> GetDownvoteCountAsync(int postId);
        Task<bool> HasUserVotedAsync(string userId, int postId);
    }
}
