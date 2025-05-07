using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ICommunityRepository : IRepository<Community>
    {
        Task<Community?> GetCommunityBySlugAsync(string slug);
        Task<IEnumerable<Community>> GetCommunitiesByCategoryAsync(int categoryId);
        Task<IEnumerable<Community>> GetPopularCommunitiesAsync(int count);
        Task<IEnumerable<Community>> GetRecentCommunitiesAsync(int count);
        Task<IEnumerable<Community>> GetTrendingCommunitiesAsync(int count);
        Task<IEnumerable<Community>> SearchCommunitiesAsync(string searchTerm);
        Task<Community?> GetCommunityWithMembersAsync(int communityId);
        Task<bool> SlugExistsAsync(string slug);
        Task UpdateMemberCountAsync(int communityId);
        Task UpdatePostCountAsync(int communityId);
    }
}
