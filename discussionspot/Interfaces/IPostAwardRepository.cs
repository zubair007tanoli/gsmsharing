using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IPostAwardRepository : IRepository<PostAward>
    {
        Task<IEnumerable<PostAward>> GetAwardsForPostAsync(int postId);
        Task<IEnumerable<PostAward>> GetAwardsGivenByUserAsync(string userId);
        Task<IEnumerable<PostAward>> GetAwardsReceivedByUserAsync(string userId);
    }
}
