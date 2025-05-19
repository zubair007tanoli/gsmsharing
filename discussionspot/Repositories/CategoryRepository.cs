using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class CategoryRepository : EfRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentCategoryId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoryTreeAsync()
        {
            var allCategories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var topLevelCategories = allCategories
                .Where(c => c.ParentCategoryId == null)
                .ToList();

            foreach (var category in topLevelCategories)
            {
                category.ChildCategories = GetChildCategories(allCategories, category.CategoryId);
            }

            return topLevelCategories;
        }

        private ICollection<Category> GetChildCategories(List<Category> allCategories, int parentId)
        {
            var children = allCategories
                .Where(c => c.ParentCategoryId == parentId)
                .ToList();

            foreach (var child in children)
            {
                child.ChildCategories = GetChildCategories(allCategories, child.CategoryId);
            }

            return children;
        }

        public async Task<Category?> GetCategoryWithSubcategoriesAsync(int categoryId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                return null;

            var allCategories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            category.ChildCategories = GetChildCategories(allCategories, categoryId);

            return category;
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Categories
                .AnyAsync(c => c.Slug == slug);
        }
    }
}