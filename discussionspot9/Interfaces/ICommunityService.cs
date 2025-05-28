using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface ICommunityService
    {
        Task<CommunityListViewModel> GetAllCommunitiesAsync(string sort = "popular", int page = 1);
        Task<CommunityDetailViewModel?> GetCommunityDetailsAsync(string slug);
        Task<CommunityDetailViewModel?> GetCommunityBySlugAsync(string slug);
        Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model);
        Task<ServiceResult> ToggleMembershipAsync(int communityId, string userId);
        Task<List<MemberViewModel>> GetCommunityMembersAsync(int communityId, int page = 1);
    }
}
