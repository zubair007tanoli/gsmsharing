using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(int categoryId, int count = 10);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}

