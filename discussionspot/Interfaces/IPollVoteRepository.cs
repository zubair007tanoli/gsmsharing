using discussionspot.Models.Domain;

namespace discussionspot.Interfaces
{
    public interface IPollVoteRepository : IRepository<PollVote>
    {
        Task<IEnumerable<PollVote>> GetVotesForOptionAsync(int pollOptionId);
        Task<IEnumerable<PollVote>> GetUserVotesForPostAsync(string userId, int postId);
        Task<bool> HasUserVotedOnPollAsync(string userId, int postId);
        Task<IEnumerable<int>> GetUserVotedOptionsAsync(string userId, int postId);
    }
}
