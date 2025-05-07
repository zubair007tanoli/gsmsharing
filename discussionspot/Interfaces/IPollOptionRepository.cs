using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IPollOptionRepository : IRepository<PollOption>
    {
        Task<IEnumerable<PollOption>> GetOptionsForPostAsync(int postId);
        Task<PollOption?> GetOptionWithVotesAsync(int pollOptionId);
        Task UpdateVoteCountAsync(int pollOptionId);
    }
}
