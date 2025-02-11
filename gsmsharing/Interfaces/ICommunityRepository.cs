using gsmsharing.Models;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gsmsharing.Interfaces
{
    public interface ICommunityRepository
    {
        Task<Community> GetByIdAsync(int id);
        Task<Community> GetBySlugAsync(string slug);
        Task<IEnumerable<Community>> GetAllAsync();
        Task<IEnumerable<SelectListItem>> GetCommunitiesForDropdownAsync();
        Task<IEnumerable<Community>> GetByCategoryIdAsync(int categoryId);
        Task<Community> CreateAsync(CommunityViewModel community, string Id);
        Task<Community> UpdateAsync(Community community);
        Task DeleteAsync(int id);
        Task<int> GetMemberCountAsync(int communityId);
        Task<bool> AddMemberAsync(int communityId, string userId);
        Task<bool> RemoveMemberAsync(int communityId, string userId);   
    }
}
