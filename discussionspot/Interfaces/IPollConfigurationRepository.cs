using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IPollConfigurationRepository : IRepository<PollConfiguration>
    {
        Task<PollConfiguration?> GetConfigForPostAsync(int postId);
        Task<IEnumerable<PollConfiguration>> GetActivePollsAsync();
        Task<IEnumerable<PollConfiguration>> GetExpiredPollsAsync();
    }
}
