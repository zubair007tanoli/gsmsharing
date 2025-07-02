using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryDetailsAsync(string categorySlug);
    }
}
