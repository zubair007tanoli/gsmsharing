using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IAwardRepository : IRepository<Award>
    {
        Task<IEnumerable<Award>> GetActiveAwardsAsync();
        Task<Award?> GetAwardWithDetailsAsync(int awardId);
    }
}
