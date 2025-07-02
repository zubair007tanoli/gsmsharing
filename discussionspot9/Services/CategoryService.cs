using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;

namespace discussionspot9.Services
{
    public class CategoryService : ICategoryService
    {
        public Task<Category> GetCategoryDetailsAsync(string categorySlug)
        {
            throw new NotImplementedException();
        }
    }
}
