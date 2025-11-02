using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace discussionspot9.Components
{
    public class StoriesStripViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<StoriesStripViewComponent> _logger;

        public StoriesStripViewComponent(ApplicationDbContext db, ILogger<StoriesStripViewComponent> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 20)
        {
            try
            {
                // Use cancellation token with timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                // Use AsNoTracking for better performance and to avoid tracking issues
                var items = await _db.Stories
                    .AsNoTracking()
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
                        CoverUrl = s.Slides.OrderBy(x => x.OrderIndex).Select(x => x.MediaUrl).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                        PosterImageUrl = s.PosterImageUrl ?? string.Empty,
                        PostUrl = s.CanonicalUrl ?? string.Empty
                    })
                    .ToListAsync(cts.Token);

                return View(items);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Stories query timed out in StoriesStripViewComponent");
                return View(new List<StoryStripItem>());
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                _logger?.LogError(sqlEx, "SQL error loading stories in StoriesStripViewComponent. Error Number: {ErrorNumber}", sqlEx.Number);
                
                // Return empty list instead of crashing the page
                return View(new List<StoryStripItem>());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading stories in StoriesStripViewComponent");
                
                // Return empty list instead of crashing the page
                return View(new List<StoryStripItem>());
            }
        }
    }

    public class StoryStripItem
    {
        public int StoryId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string PosterImageUrl { get; set; } = string.Empty;
        public string PostUrl { get; set; } = string.Empty;
    }
}


