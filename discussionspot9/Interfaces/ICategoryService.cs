using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using System.Collections.Generic;

namespace discussionspot9.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryDetailsAsync(string categorySlug);
        Task<List<CategoryTreeViewModel>> GetAllCategoriesAsync();
        Task<CategoryCommunitiesViewModel> GetCategoryCommunitiesAsync(string categorySlug, int page = 1);
    }
}
