using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Components
{
    public class StoriesStripViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public StoriesStripViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 20)
        {
            var items = await _db.Stories
                .Include(s => s.User)
                .Include(s => s.Slides)
                .Where(s => s.Status == "published")
                .OrderByDescending(s => s.UpdatedAt)
                .Take(count)
                .Select(s => new StoryStripItem
                {
                    StoryId = s.StoryId,
                    Slug = s.Slug ?? string.Empty,
                    Title = s.Title ?? string.Empty,
                    Author = s.User != null ? (s.User.UserName ?? "User") : "User",
                    CoverUrl = s.Slides.OrderBy(x => x.OrderIndex).Select(x => x.MediaUrl).FirstOrDefault() ?? string.Empty,
                    PostUrl = s.CanonicalUrl ?? string.Empty
                })
                .ToListAsync();

            return View(items);
        }
    }

    public class StoryStripItem
    {
        public int StoryId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string PostUrl { get; set; } = string.Empty;
    }
}


