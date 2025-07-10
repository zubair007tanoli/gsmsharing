using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
   
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }    

        public async Task<Category> GetCategoryDetailsAsync(string categorySlug)
        {
            try
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.IsActive);

                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with slug {categorySlug} not found or is inactive.");
                }

                return category;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                throw;
            }
        }

        public async Task<List<CategoryTreeViewModel>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .Include(c => c.Communities.Where(com => !com.IsDeleted))
                    .Include(c => c.ChildCategories.Where(cc => cc.IsActive))
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var categoryTree = categories
                    .Where(c => c.ParentCategoryId == null)
                    .Select(c => MapToCategoryTreeViewModel(c, categories))
                    .ToList();

                return categoryTree;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                return new List<CategoryTreeViewModel>();
            }
        }

        public async Task<CategoryCommunitiesViewModel> GetCategoryCommunitiesAsync(string categorySlug, int page = 1)
        {
            try
            {
                const int pageSize = 25;
                var skip = (page - 1) * pageSize;

                var category = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Slug == categorySlug && c.IsActive)
                    .Select(c => new { c.CategoryId, c.Name, c.Slug, c.Description })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with slug {categorySlug} not found or is inactive.");
                }

                var communitiesQuery = _context.Communities
                    .AsNoTracking()
                    .Where(c => c.CategoryId == category.CategoryId && !c.IsDeleted)
                    .Select(c => new CommunityViewModel
                    {
                        CommunityId = c.CommunityId,
                        Name = c.Name,
                        Slug = c.Slug,
                        ShortDescription = c.ShortDescription ?? string.Empty,
                        IconUrl = c.IconUrl,
                        MemberCount = c.MemberCount,
                        PostCount = c.PostCount
                    })
                    .OrderBy(c => c.Name);

                var totalCommunities = await communitiesQuery.CountAsync();
                var communities = await communitiesQuery
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                return new CategoryCommunitiesViewModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Slug = category.Slug,
                    Description = category.Description ?? string.Empty,
                    Communities = communities,
                    TotalCommunities = totalCommunities,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalCommunities / (double)pageSize)
                };
            }
            catch (Exception ex)
            {              
                ex.Message.ToString();
                throw;
            }
        }

        private CategoryTreeViewModel MapToCategoryTreeViewModel(Category category, List<Category> allCategories)
        {
            return new CategoryTreeViewModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description ?? string.Empty,
                Communities = category.Communities
                    .Select(c => new CommunityViewModel
                    {
                        CommunityId = c.CommunityId,
                        Name = c.Name,
                        Slug = c.Slug,
                        ShortDescription = c.ShortDescription ?? string.Empty,
                        IconUrl = c.IconUrl,
                        MemberCount = c.MemberCount,
                        PostCount = c.PostCount
                    })
                    .OrderBy(c => c.Name)
                    .ToList(),
                ChildCategories = allCategories
                    .Where(c => c.ParentCategoryId == category.CategoryId)
                    .Select(c => MapToCategoryTreeViewModel(c, allCategories))
                    .OrderBy(c => c.Name)
                    .ToList()
            };
        }

    }
}
