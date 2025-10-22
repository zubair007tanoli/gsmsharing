using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace discussionspot9.Controllers
{
    public class StoriesController : Controller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            UserManager<IdentityUser> userManager,
            ILogger<StoriesController> logger)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /stories
        [ResponseCache(Duration = 300, VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index(string? category = null, string? sort = "newest", int page = 1)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Stories
                .Include(s => s.User)
                .Include(s => s.Community)
                .Include(s => s.Slides)
                .Where(s => s.Status == "published");

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(s => s.Community != null && s.Community.Slug == category);
            }

            // Apply sorting
            query = sort switch
            {
                "popular" => query.OrderByDescending(s => s.ViewCount),
                "trending" => query.OrderByDescending(s => s.ViewCount).ThenByDescending(s => s.PublishedAt),
                "oldest" => query.OrderBy(s => s.PublishedAt),
                _ => query.OrderByDescending(s => s.PublishedAt)
            };

            var stories = await query
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            ViewData["Category"] = category;
            ViewData["Sort"] = sort;
            ViewData["Page"] = page;

            return View(stories);
        }

        // GET: /stories/{slug}
        [ResponseCache(Duration = 600, VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> ViewStory(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            using var context = _contextFactory.CreateDbContext();
            var story = await context.Stories
                .Include(s => s.User)
                .Include(s => s.Community)
                .Include(s => s.Slides)
                    .ThenInclude(sl => sl.Media)
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (story == null)
                return NotFound();

            // Increment view count
            story.ViewCount++;
            await context.SaveChangesAsync();

            // Set SEO metadata
            ViewData["Title"] = story.Title;
            ViewData["Description"] = story.MetaDescription ?? story.Description ?? $"Read {story.Title} on DiscussionSpot";
            ViewData["CanonicalUrl"] = story.CanonicalUrl ?? Url.Action("View", "Stories", new { slug }, Request.Scheme);
            ViewData["OgImage"] = story.PosterImageUrl;
            ViewData["OgType"] = "article";

            return View(story);
        }

        // GET: /stories/viewer/{slug}
        [ResponseCache(Duration = 600, VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Viewer(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            using var context = _contextFactory.CreateDbContext();
            var story = await context.Stories
                .Include(s => s.User)
                .Include(s => s.Community)
                .Include(s => s.Slides)
                    .ThenInclude(sl => sl.Media)
                .FirstOrDefaultAsync(s => s.Slug == slug && s.Status == "published");

            if (story == null)
                return NotFound();

            // Increment view count
            story.ViewCount++;
            await context.SaveChangesAsync();

            // Set SEO metadata
            ViewData["Title"] = story.Title;
            ViewData["Description"] = story.MetaDescription ?? story.Description ?? $"Read {story.Title} on DiscussionSpot";
            ViewData["CanonicalUrl"] = story.CanonicalUrl ?? Url.Action("Viewer", "Stories", new { slug }, Request.Scheme);
            ViewData["OgImage"] = story.PosterImageUrl;
            ViewData["OgType"] = "article";

            return View(story);
        }

        // GET: /stories/amp/{slug}
        [ResponseCache(Duration = 3600, VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Amp(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            using var context = _contextFactory.CreateDbContext();
            var story = await context.Stories
                .Include(s => s.User)
                .Include(s => s.Community)
                .Include(s => s.Slides)
                    .ThenInclude(sl => sl.Media)
                .FirstOrDefaultAsync(s => s.Slug == slug && s.Status == "published" && s.IsAmpEnabled);

            if (story == null)
                return NotFound();

            // Set AMP-specific metadata
            ViewData["Title"] = story.Title;
            ViewData["Description"] = story.MetaDescription ?? story.Description ?? $"Read {story.Title} on DiscussionSpot";
            ViewData["CanonicalUrl"] = Url.Action("View", "Stories", new { slug }, Request.Scheme);
            ViewData["AmpUrl"] = Url.Action("Amp", "Stories", new { slug }, Request.Scheme);
            ViewData["PublisherName"] = story.PublisherName ?? "DiscussionSpot";
            ViewData["PublisherLogo"] = story.PublisherLogo ?? Url.Content("~/favicon.ico");
            ViewData["PosterImage"] = story.PosterImageUrl;

            return View(story);
        }

        // GET: /stories/create
        [Authorize]
        public IActionResult Create()
        {
            var model = new CreateStoryViewModel();
            // Add one default slide
            model.Slides.Add(new CreateStorySlideViewModel
            {
                OrderIndex = 0,
                SlideType = "media",
                Duration = 5000,
                BackgroundColor = "#000000",
                TextColor = "#ffffff",
                Alignment = "center",
                FontSize = "24px"
            });
            return View(model);
        }

        // GET: /stories/editor
        [Authorize]
        public IActionResult Editor()
        {
            var model = new CreateStoryViewModel();
            return View(model);
        }

        // GET: /stories/info
        public IActionResult Info()
        {
            return View();
        }

        // POST: /stories/create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStoryViewModel model)
        {
            _logger.LogInformation($"Story creation attempt - ModelState.IsValid: {ModelState.IsValid}");
            
            // Log all model state errors for debugging
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning($"ModelState validation failed. Errors: {string.Join(", ", errors)}");
                
                // Log each field's state
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (!state.Errors.Any()) continue;
                    
                    _logger.LogWarning($"Field '{key}' has errors: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                }
                
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found during story creation");
                return Unauthorized();
            }

            try
            {
                // Ensure all required fields are set
                if (string.IsNullOrEmpty(model.Title))
                {
                    ModelState.AddModelError("Title", "Title is required");
                    return View(model);
                }

                using var context = _contextFactory.CreateDbContext();

                // Create the story entity
                var story = new Story
                {
                    Title = model.Title,
                    Description = model.Description,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    PublisherName = model.PublisherName,
                    PosterImageUrl = model.PosterImageUrl,
                    PublisherLogo = model.PublisherLogo,
                    CanonicalUrl = model.CanonicalUrl,
                    Status = model.Status,
                    IsAmpEnabled = model.IsAmpEnabled,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Slug = GenerateSlug(model.Title),
                    ViewCount = 0,
                    SlideCount = 0,
                    TotalDuration = 0
                };

                context.Stories.Add(story);
                await context.SaveChangesAsync();

                // Create slides if any
                if (model.Slides != null && model.Slides.Any())
                {
                    var slides = model.Slides.Select((slide, index) => new StorySlide
                    {
                        StoryId = story.StoryId,
                        MediaId = slide.MediaId,
                        Caption = slide.Caption,
                        Headline = slide.Headline,
                        Text = slide.Text,
                        Duration = slide.Duration,
                        OrderIndex = index,
                        SlideType = slide.SlideType,
                        BackgroundColor = slide.BackgroundColor,
                        TextColor = slide.TextColor,
                        FontSize = slide.FontSize,
                        Alignment = slide.Alignment,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                    context.StorySlides.AddRange(slides);
                    
                    // Update story with slide count and duration
                    story.SlideCount = slides.Count;
                    story.TotalDuration = slides.Sum(s => s.Duration);
                    await context.SaveChangesAsync();
                }

                _logger.LogInformation($"Story created successfully with ID: {story.StoryId}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating story");
                ModelState.AddModelError("", "An error occurred while creating the story. Please try again.");
                return View(model);
            }
        }

        // GET: /stories/edit/{id}
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            using var context = _contextFactory.CreateDbContext();
            var story = await context.Stories
                .Include(s => s.Slides)
                    .ThenInclude(sl => sl.Media)
                .FirstOrDefaultAsync(s => s.StoryId == id && s.UserId == user.Id);

            if (story == null)
                return NotFound();

            return View(story);
        }

        // POST: /stories/publish/{id}
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            using var context = _contextFactory.CreateDbContext();
            var story = await context.Stories
                .FirstOrDefaultAsync(s => s.StoryId == id && s.UserId == user.Id);

            if (story == null)
                return NotFound();

            story.Status = "published";
            story.PublishedAt = DateTime.UtcNow;
            story.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return RedirectToAction("View", new { slug = story.Slug });
        }

        // API: Get story data for viewer
        [HttpGet]
        public async Task<IActionResult> GetStoryData(string slug)
        {
            using var context = _contextFactory.CreateDbContext();
            var story = await context.Stories
                .Include(s => s.Slides)
                    .ThenInclude(sl => sl.Media)
                .FirstOrDefaultAsync(s => s.Slug == slug && s.Status == "published");

            if (story == null)
                return NotFound();

            var storyData = new
            {
                story.StoryId,
                story.Title,
                story.Slug,
                story.Description,
                story.TotalDuration,
                story.SlideCount,
                slides = story.Slides.OrderBy(s => s.OrderIndex).Select(s => new
                {
                    s.StorySlideId,
                    s.Caption,
                    s.Headline,
                    s.Text,
                    s.Duration,
                    s.SlideType,
                    s.BackgroundColor,
                    s.TextColor,
                    s.FontSize,
                    s.Alignment,
                    media = s.Media != null ? new
                    {
                        s.Media.Url,
                        s.Media.ThumbnailUrl,
                        s.Media.MediaType,
                        s.Media.Width,
                        s.Media.Height
                    } : null
                })
            };

            return Json(storyData);
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return Guid.NewGuid().ToString("N")[..8];

            var slug = title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("\"", "")
                .Replace("'", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("|", "-")
                .Replace("+", "-")
                .Replace("=", "-")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("&", "")
                .Replace("*", "")
                .Replace("~", "")
                .Replace("`", "");

            // Remove multiple consecutive dashes
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            // Remove leading/trailing dashes
            slug = slug.Trim('-');

            // Ensure slug is not empty
            if (string.IsNullOrEmpty(slug))
                slug = Guid.NewGuid().ToString("N")[..8];

            // Add timestamp to ensure uniqueness
            return $"{slug}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}
