using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
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

        public async Task<IActionResult> IndexAsync()
        {
            var Cat = await GetCategoriesWithCommunitiesAndPosts();
            return View(Cat);
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
