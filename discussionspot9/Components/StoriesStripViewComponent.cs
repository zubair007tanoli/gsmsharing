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

        public async Task<IViewComponentResult> InvokeAsync(int count = 10)
        {
            try
            {
                // Use cancellation token with timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                // Use AsNoTracking for better performance and to avoid tracking issues
                // Load random published stories from all users
                
                // First, get all published story IDs
                var allStoryIds = await _db.Stories
                    .AsNoTracking()
                    .Where(s => s.Status == "published")
                    .Select(s => s.StoryId)
                    .ToListAsync(cts.Token);

                // Shuffle them in memory for true randomization
                var random = new Random();
                var randomStoryIds = allStoryIds.OrderBy(_ => random.Next()).Take(count).ToList();

                if (!randomStoryIds.Any())
                {
                    _logger?.LogInformation("No published stories found");
                    return View(new List<StoryStripItem>());
                }

                // Now fetch the full story data for the random IDs
                var items = await _db.Stories
                    .AsNoTracking()
                    .Include(s => s.User)
                    .Include(s => s.Slides)
                    .Include(s => s.Post) // Include post for better URL linking
                        .ThenInclude(p => p.Community)
                    .Where(s => randomStoryIds.Contains(s.StoryId))
                    .Select(s => new StoryStripItem
                    {
                        StoryId = s.StoryId,
                        Slug = s.Slug ?? string.Empty,
                        Title = s.Title ?? string.Empty,
                        Author = s.User != null ? (s.User.UserName ?? "User") : "User",
                        // Prioritize: PosterImageUrl > First slide media > Empty string (gradient will be used)
                        CoverUrl = !string.IsNullOrWhiteSpace(s.PosterImageUrl) 
                            ? s.PosterImageUrl 
                            : (s.Slides != null && s.Slides.Any()
                                ? s.Slides.OrderBy(x => x.OrderIndex).Select(x => x.MediaUrl).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty
                                : string.Empty),
                        PosterImageUrl = s.PosterImageUrl ?? string.Empty,
                        PostUrl = s.Post != null && s.Post.Community != null 
                            ? $"/r/{s.Post.Community.Slug}/posts/{s.Post.Slug}"
                            : (s.CanonicalUrl ?? string.Empty),
                        ViewCount = s.ViewCount
                    })
                    .ToListAsync(cts.Token);

                // Maintain random order
                items = items.OrderBy(s => randomStoryIds.IndexOf(s.StoryId)).ToList();

                _logger?.LogInformation("Loaded {Count} random stories for StoriesStrip", items.Count);
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
        public int ViewCount { get; set; }
    }
}


