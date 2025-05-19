using discussionspot.Models.Domain;
using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface ICommunityService
    {
        Task<CommunityViewModel> GetCommunityByIdAsync(int communityId);
        Task<Community> GetCommunityEntityAsync(int communityId);
        Task<CommunityViewModel> GetCommunityBySlugAsync(string slug);
        Task<IEnumerable<CommunityViewModel>> GetCommunitiesAsync();
        Task<IEnumerable<CommunityViewModel>> GetPopularCommunitiesAsync(int count);
        Task<int> CreateCommunityAsync(CommunityCreateViewModel model, string userId);
        Task<bool> UpdateCommunityAsync(int communityId, CommunityCreateViewModel model, string userId);
        Task<bool> JoinCommunityAsync(int communityId, string userId);
        Task<bool> LeaveCommunityAsync(int communityId, string userId);
        Task<bool> IsMemberAsync(int communityId, string userId);
    }
}
