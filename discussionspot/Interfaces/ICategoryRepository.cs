using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentCategoryId);
        Task<IEnumerable<Category>> GetCategoryTreeAsync();
        Task<Category?> GetCategoryWithSubcategoriesAsync(int categoryId);
        Task<bool> SlugExistsAsync(string slug);
    }
}
