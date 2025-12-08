using GsmsharingV2.DTOs;

namespace GsmsharingV2.Interfaces
{
    public interface ICommunityService
    {
        Task<CommunityDto?> GetByIdAsync(int id);
        Task<CommunityDto?> GetBySlugAsync(string slug);
        Task<IEnumerable<CommunityDto>> GetAllAsync();
        Task<IEnumerable<CommunityDto>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<CommunityDto>> GetByUserIdAsync(string userId);
        Task<CommunityDto> CreateAsync(CreateCommunityDto createCommunityDto, string userId);
        Task<CommunityDto> UpdateAsync(UpdateCommunityDto updateCommunityDto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<bool> JoinCommunityAsync(int communityId, string userId);
        Task<bool> LeaveCommunityAsync(int communityId, string userId);
        Task<bool> IsMemberAsync(int communityId, string userId);
    }
}

