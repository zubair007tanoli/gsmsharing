using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ICommunityMemberRepository : IRepository<CommunityMember>
    {
        Task<CommunityMember?> GetMembershipAsync(string userId, int communityId);
        Task<IEnumerable<CommunityMember>> GetCommunityMembersAsync(int communityId);
        Task<IEnumerable<CommunityMember>> GetUserMembershipsAsync(string userId);
        Task<IEnumerable<CommunityMember>> GetModeratorsAsync(int communityId);
        Task<bool> IsMemberAsync(string userId, int communityId);
        Task<bool> IsModeratorAsync(string userId, int communityId);
        Task<bool> IsAdminAsync(string userId, int communityId);
    }
}
