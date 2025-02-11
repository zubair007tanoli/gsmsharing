using gsmsharing.Models;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gsmsharing.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetAllAsync();   
        Task<IEnumerable<SelectListItem>> CreateCategorySelectListAsync();
        Task<IEnumerable<CategoryViewModel>> GetHierarchicalCategoriesAsync();
        Task<IEnumerable<CategoryViewModel>> GetHierarchicalCategoriesTestAsync();
        Task<IEnumerable<Category>> GetByParentIdAsync(int parentId);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Category>> GetPaginatedAsync(int page, int pageSize);
    }
}
