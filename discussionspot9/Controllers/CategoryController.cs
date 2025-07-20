using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext DbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        [HttpGet("categories", Name = "categories_list")]
        public async Task<IActionResult> IndexAsync()
        {
            var Cat = await GetCategoriesWithCommunitiesAndPosts();
            return View(Cat);
        }

        public async Task<IActionResult> CategoryDetails(string slug) 
        { 
            var Data = await GetCategoryDetailsBySlug(slug); // Example category ID, replace with actual logic to get ID
            return View(Data);
        }
        private async Task<CategoryDetailsViewModel> GetCategoryDetailsBySlug(string slug)
        {
            var category = await DbContext.Categories
                .Include(c => c.Communities.Where(com => !com.IsDeleted))
                    .ThenInclude(com => com.Posts.Where(p => p.Status == "published"))
                .Include(c => c.ChildCategories.Where(child => child.IsActive))
                    .ThenInclude(child => child.Communities.Where(com => !com.IsDeleted))
                        .ThenInclude(com => com.Posts.Where(p => p.Status == "published"))
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (category == null)
                return null;

            // Get related categories (siblings)
            var relatedCategories = await DbContext.Categories
                .Where(c => c.ParentCategoryId == category.ParentCategoryId &&
                           c.Slug != slug &&
                           c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Calculate statistics
            var allCommunities = category.Communities.ToList();

            // Include communities from child categories
            foreach (var child in category.ChildCategories)
            {
                allCommunities.AddRange(child.Communities);
            }

            var totalMembers = await DbContext.CommunityMembers
                .Where(cm => allCommunities.Select(c => c.CommunityId).Contains(cm.CommunityId))
                .CountAsync();

            var totalPosts = allCommunities.Sum(c => c.PostCount);

            var weeklyActivity = await DbContext.Posts
                .Where(p => allCommunities.Select(c => c.CommunityId).Contains(p.CommunityId) &&
                           p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            return new CategoryDetailsViewModel
            {
                Category = category,
                Communities = allCommunities,
                RelatedCategories = relatedCategories,
                TotalMembers = totalMembers,
                TotalPosts = totalPosts,
                WeeklyActivity = weeklyActivity
            };
        }
        private async Task<List<Category>> GetCategoriesWithCommunitiesAndPosts()
        {
            return await DbContext.Categories
                .Include(c => c.Communities)
                    .ThenInclude(com => com.Posts)
                .Include(c => c.ChildCategories)
                    .ThenInclude(child => child.Communities)
                        .ThenInclude(com => com.Posts)
                .Where(c => c.ParentCategoryId == null) // Root categories only
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }
    }
}
