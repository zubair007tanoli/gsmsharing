using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface ICommunityService
    {
        //Task<(List<CommunityDropdownViewModel> PopularCommunities, List<CommunityDropdownViewModel> UserCommunities)> GetCommunitiesForDropdownAsync();
        Task<CommunityListViewModel> GetAllCommunitiesAsync(string sort = "popular", int page = 1);
        Task<CommunityDetailViewModel?> GetCommunityDetailsAsync(string slug);
        Task<CommunityDetailViewModel?> GetCommunityBySlugAsync(string slug);
        Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model);
        Task<ServiceResult> ToggleMembershipAsync(int communityId, string userId);
        Task<List<MemberViewModel>> GetCommunityMembersAsync(int communityId, int page = 1);
        Task<bool> IsCommunityMemberAsync(int communityId, string userId);
        Task<bool> IsCommunityAdminAsync(int communityId, string userId);
        Task<bool> IsCommunityModeratorAsync(int communityId, string userId);
        Task<ServiceResult> UpdateCommunityDetailsAsync(CommunityDetailViewModel model);
        Task<ServiceResult> DeleteCommunityAsync(int communityId, string userId);
        Task<ServiceResult> BanUserFromCommunityAsync(int communityId, string userIdToBan, string moderatorId);
        Task<ServiceResult> PromoteDemoteCommunityMemberAsync(int communityId, string userId, string newRole, string moderatorId);
        Task<string?> GetCommunityMemberRoleAsync(int communityId, string userId);

        // New methods for CreatePostTest
        Task<List<CommunityViewModel>> GetUserJoinedCommunitiesAsync(string userId);
        Task<List<CommunityViewModel>> GetSuggestedCommunitiesAsync(string userId, int count = 5);
    }
}
