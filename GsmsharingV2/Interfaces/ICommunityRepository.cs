using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface ICommunityRepository
    {
        Task<Community?> GetByIdAsync(int id);
        Task<Community?> GetBySlugAsync(string slug);
        Task<IEnumerable<Community>> GetAllAsync();
        Task<Community> CreateAsync(Community community);
        Task<Community> UpdateAsync(Community community);
        Task DeleteAsync(int id);
    }
}

