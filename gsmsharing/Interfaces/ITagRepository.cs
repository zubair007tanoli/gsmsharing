using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface ITagRepository
    {
        Task<Tags> GetByIdAsync(int id);
        Task<Tags> GetBySlugAsync(string slug);
        Task<IEnumerable<Tags>> GetAllAsync();
        Task<Tags> CreateAsync(Tags tag);
        Task<Tags> UpdateAsync(Tags tag);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Tags>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<Tags>> GetByPostIdAsync(int postId);
    }
}
