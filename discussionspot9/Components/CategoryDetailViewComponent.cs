using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this using directive

namespace discussionspot9.Components
{
    public class CategoryDetailViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryDetailViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await GetNavigationCategories();
            return View(categories);
        }

        private async Task<List<CategoryNavItem>> GetNavigationCategories()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryNavItem
                {
                    Name = c.Name,
                    Slug = c.Slug,
                    Icon = GetDefaultIcon(c.Name)
                })
                .ToListAsync(); // This now works because of the added using directive
        }

        private string GetDefaultIcon(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "technology" => "fas fa-laptop-code",
                "gaming" => "fas fa-gamepad",
                "creative arts" => "fas fa-palette",
                "science" => "fas fa-flask",
                "sports" => "fas fa-football-ball",
                "music" => "fas fa-music",
                "movies" => "fas fa-film",
                "books" => "fas fa-book",
                _ => "fas fa-folder"
            };
        }
    }
    public class CategoryNavItem
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public required string Icon { get; set; }
    }
}
