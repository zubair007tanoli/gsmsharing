using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Communities)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.Communities)
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Communities)
                .OrderBy(c => c.DisplayOrder ?? 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryID == null && (c.IsActive == true || c.IsActive == null))
                .Include(c => c.Communities)
                .OrderBy(c => c.DisplayOrder ?? 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive == true || c.IsActive == null)
                .Include(c => c.Communities)
                .OrderBy(c => c.DisplayOrder ?? 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(int categoryId, int count = 10)
        {
            return await _context.Posts
                .Include(p => p.Community)
                    .ThenInclude(c => c.Category)
                .Include(p => p.User)
                .Where(p => p.Community != null && 
                           p.Community.CategoryID == categoryId && 
                           (p.PostStatus == "published" || p.PostStatus == null))
                .OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}

