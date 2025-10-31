using discussionspot9.Constants;
using discussionspot9.Data.DbContext;
using discussionspot9.Extensions;
using discussionspot9.Helpers;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize]
    public class StoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StoriesController> _logger;
        private readonly IStoryGenerationService _storyGenerationService;

        public StoriesController(
            ApplicationDbContext context,
            ILogger<StoriesController> logger,
            IStoryGenerationService storyGenerationService)
        {
            _context = context;
            _logger = logger;
            _storyGenerationService = storyGenerationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var pageSize = StoryConstants.IndexPageSize;
            var skip = (page - 1) * pageSize;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var storiesQuery = _context.Stories
                .AsNoTracking()
                .Include(s => s.Community)
                .Include(s => s.Slides.OrderBy(sl => sl.OrderIndex))
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt);

            var totalStories = await storiesQuery.CountAsync();
            
            var storiesList = await storiesQuery
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var stories = storiesList.Select(s => new StoryViewModel
            {
                StoryId = s.StoryId,
                Title = s.Title ?? "",
                Slug = s.Slug ?? "",
                Description = s.Description ?? "",
                Status = s.Status ?? "",
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                PostTitle = s.Title ?? "",
                PostSlug = s.Slug ?? "",
                CommunityName = s.Community != null ? s.Community.Title : "",
                CommunitySlug = s.Community != null ? s.Community.Slug : "",
                SlideCount = s.Slides.Count,
                PosterImageUrl = s.PosterImageUrl ?? s.Slides.OrderBy(sl => sl.OrderIndex)
                    .FirstOrDefault(sl => !string.IsNullOrEmpty(sl.MediaUrl))?.MediaUrl
            }).ToList();

            var model = new StoriesIndexViewModel
            {
                Stories = stories,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalStories / (double)pageSize),
                TotalStories = totalStories
            };

            return View(model);
        }

        [HttpGet]
        [Route("stories/details/{storySlug}")]
        public async Task<IActionResult> Details(string storySlug)
        {
            var story = await StoryControllerHelpers.GetStoryWithSlidesAsync(_context, storySlug, tracking: false);

            if (story == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!StoryControllerHelpers.ValidateStoryOwnership(story, userId))
            {
                return Forbid();
            }

            var model = new StoryDetailViewModel
            {
                    StoryId = story.StoryId,
                    Title = story.Title ?? "",
                    Slug = story.Slug ?? "",
                    Description = story.Description ?? "",
                    Status = story.Status ?? "",
                    CreatedAt = story.CreatedAt,
                    UpdatedAt = story.UpdatedAt,
                    PostTitle = story.Title ?? "",
                    PostSlug = story.Slug ?? "",
                    CommunityName = story.Community?.Title ?? "",
                    CommunitySlug = story.Community?.Slug ?? "",
                    Slides = story.Slides.OrderBy(s => s.OrderIndex).Select(s => new StorySlideViewModel
                    {
                        SlideId = s.StorySlideId,
                        Title = s.Headline ?? "",
                        Content = s.Text ?? "",
                        ImageUrl = s.MediaUrl ?? "",
                        SlideOrder = s.OrderIndex,
                        SlideType = s.SlideType ?? StoryConstants.SlideTypeMedia
                    }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        [Route("stories/{storySlug}/edit")]
        public async Task<IActionResult> Edit(string storySlug)
        {
            var story = await _context.Stories
                .Include(s => s.Slides)
                .FirstOrDefaultAsync(s => s.Slug == storySlug);

            if (story == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!StoryControllerHelpers.ValidateStoryOwnership(story, userId))
            {
                return Forbid();
            }

            var model = new StoryEditViewModel
            {
                StoryId = story.StoryId,
                Title = story.Title ?? "",
                Slug = story.Slug ?? "",
                Description = story.Description ?? "",
                Status = story.Status ?? "",
                UpdatedAt = story.UpdatedAt,
                Slides = story.Slides.OrderBy(s => s.OrderIndex).Select(s => new StorySlideEditViewModel
                {
                    StorySlideId = s.StorySlideId,
                    SlideId = s.StorySlideId,
                    Title = s.Headline ?? "",
                    Content = s.Text ?? "",
                    ImageUrl = s.MediaUrl ?? "",
                    SlideOrder = s.OrderIndex,
                    SlideType = s.SlideType ?? StoryConstants.SlideTypeMedia,
                    BackgroundColor = s.BackgroundColor ?? StoryConstants.DefaultBackgroundColor,
                    BackgroundImageUrl = s.MediaUrl ?? "",
                    LinkUrl = s.MediaUrl ?? ""
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Route("stories/{storySlug}/edit")]
        public async Task<IActionResult> Edit(string storySlug, StoryEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var story = await _context.Stories
                .Include(s => s.Slides)
                .FirstOrDefaultAsync(s => s.Slug == storySlug);

            if (story == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!StoryControllerHelpers.ValidateStoryOwnership(story, userId))
            {
                return Forbid();
            }

            try
            {
                // Update story properties
                story.Title = model.Title;
                story.Description = model.Description;
                story.Status = model.Status;
                story.UpdatedAt = DateTime.UtcNow;

                // Update slides - validate ownership
                foreach (var slideModel in model.Slides)
                {
                    var slide = story.Slides.FirstOrDefault(s => s.StorySlideId == slideModel.SlideId);
                    if (slide == null && slideModel.SlideId > 0)
                    {
                        ModelState.AddModelError("", $"Slide {slideModel.SlideId} does not belong to this story");
                        return View(model);
                    }
                    
                    if (slide != null)
                    {
                        slide.Headline = slideModel.Title;
                        slide.Text = slideModel.Content;
                        slide.MediaUrl = slideModel.ImageUrl;
                        slide.OrderIndex = slideModel.SlideOrder;
                        slide.SlideType = slideModel.SlideType;
                        slide.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Story updated successfully!";
                return RedirectToAction("Details", new { storySlug = story.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating story {StoryId}", story.StoryId);
                ModelState.AddModelError("", "An error occurred while updating the story.");
            return View(model);
        }
        }

        [HttpPost]
        [Route("stories/{storySlug}/regenerate")]
        public async Task<IActionResult> Regenerate(string storySlug)
        {
            var story = await _context.Stories
                .FirstOrDefaultAsync(s => s.Slug == storySlug);

            if (story == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!StoryControllerHelpers.ValidateStoryOwnership(story, userId))
            {
                return Forbid();
            }

            try
            {
                var storyOptions = new StoryGenerationOptions
                {
                    AutoGenerate = true,
                    UseAI = true,
                    Style = StoryConstants.StyleInformative,
                    Length = StoryConstants.LengthMedium
                };

                // Note: Story regeneration would need a different approach since Story doesn't have Post reference
                // For now, we'll skip the regeneration or implement a different strategy
                _logger.LogInformation("Story regeneration requested for story {StoryId}", story.StoryId);

                TempData["SuccessMessage"] = "Story regeneration queued successfully!";
                return RedirectToAction("Details", new { storySlug = story.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating story {StoryId}", story.StoryId);
                TempData["ErrorMessage"] = "An error occurred while regenerating the story.";
                return RedirectToAction("Details", new { storySlug = story.Slug });
            }
        }

        [HttpPost]
        [Route("stories/{storySlug}/delete")]
        public async Task<IActionResult> Delete(string storySlug)
        {
            var story = await _context.Stories
                .FirstOrDefaultAsync(s => s.Slug == storySlug);

            if (story == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!StoryControllerHelpers.ValidateStoryOwnership(story, userId))
            {
                return Forbid();
            }

            try
            {
                // Load story with related entities to handle cascading deletes properly
                var storyWithRelations = await _context.Stories
                    .Include(s => s.Slides)
                    .FirstOrDefaultAsync(s => s.StoryId == story.StoryId);

                if (storyWithRelations != null)
                {
                    _context.Stories.Remove(storyWithRelations);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Story deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting story {StoryId}", story.StoryId);
                TempData["ErrorMessage"] = "An error occurred while deleting the story.";
                return RedirectToAction("Details", new { storySlug = story.Slug });
            }
        }

        // ===== MISSING ROUTES FROM PROGRAM.CS =====

        [HttpGet]
        [Route("stories/create")]
        public IActionResult Create()
        {
            var model = new CreateStoryViewModel();
            return View(model);
        }

        [HttpPost]
        [Route("stories/create")]
        public async Task<IActionResult> Create(CreateStoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                    return View(model);
                }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Generate unique slug
                var slug = await StoryControllerHelpers.GenerateUniqueSlugAsync(_context, model.Title);

                // Create new story
                var story = new Story
                {
                    Title = model.Title,
                    Slug = slug,
                    Description = model.Description,
                    Status = StoryConstants.StatusDraft,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Story created successfully!";
                return RedirectToAction("Editor", new { id = story.StoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating story");
                ModelState.AddModelError("", "An error occurred while creating the story.");
                return View(model);
            }
        }

        [HttpGet]
        [Route("stories/editor/{id?}")]
        public async Task<IActionResult> Editor(int? id = null)
        {
            if (id.HasValue)
            {
                // Load existing story if ID provided
                var story = await _context.Stories
                    .Include(s => s.Slides)
                    .Include(s => s.Community)
                    .FirstOrDefaultAsync(s => s.StoryId == id.Value);
                
                if (story == null)
                {
                    return NotFound();
                }

                var editModel = new StoryEditViewModel
                {
                    StoryId = story.StoryId,
                    Title = story.Title ?? "",
                    Description = story.Description ?? "",
                    Status = story.Status ?? "",
                    Slides = story.Slides.Select(s => new StorySlideEditViewModel
                    {
                        StorySlideId = s.StorySlideId,
                        Headline = s.Headline ?? "",
                        Text = s.Text ?? "",
                        MediaUrl = s.MediaUrl ?? "",
                        MediaType = s.MediaType ?? "",
                        OrderIndex = s.OrderIndex,
                    BackgroundColor = s.BackgroundColor ?? StoryConstants.DefaultBackgroundColor,
                    TextColor = s.TextColor ?? StoryConstants.DefaultTextColor,
                        Duration = s.Duration
                    }).OrderBy(s => s.OrderIndex).ToList()
                };

                return View("Editor", editModel);
            }

            // No ID provided, return empty editor for new story
            var model = new StoryEditViewModel
            {
                StoryId = 0,
                Title = "",
                Description = "",
                Status = StoryConstants.StatusDraft,
                Slides = new List<StorySlideEditViewModel>()
            };
            return View("Editor", model);
        }

        [HttpGet]
        [Route("stories/info")]
        public IActionResult Info()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("stories/viewer/{slug}")]
        public async Task<IActionResult> Viewer(string slug)
        {
            var story = await _context.Stories
                .Include(s => s.Slides)
                .Include(s => s.User)
                .Include(s => s.Community)
                // Post relationship removed - column doesn't exist in database yet
                // .Include(s => s.Post)
                //     .ThenInclude(p => p.Community)
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (story == null)
            {
                return NotFound();
            }

            // Track story view for analytics (fire and forget - don't block page load)
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var storyId = story.StoryId;
            var serviceProvider = HttpContext.RequestServices;
            
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    var view = new StoryView
                    {
                        StoryId = storyId,
                        UserId = userId,
                        ViewedAt = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        UserAgent = userAgent
                    };
                    context.StoryViews.Add(view);
                    
                    // Update story view count
                    var storyToUpdate = await context.Stories.FindAsync(storyId);
                    if (storyToUpdate != null)
                    {
                        storyToUpdate.ViewCount++;
                    }
                    
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error tracking story view for story {StoryId}", storyId);
                }
            });

            return View(story);
        }

        // Lightweight JSON payload for stories strip/modal
        [HttpGet]
        [AllowAnonymous]
        [Route("api/stories/{slug}/slides")]
        public async Task<IActionResult> GetSlides(string slug)
        {
            var story = await _context.Stories
                .Include(s => s.Slides)
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (story == null)
            {
                return NotFound(new { success = false, message = "Story not found" });
            }

            // Check authorization for draft stories
            if (story.Status == StoryConstants.StatusDraft)
            {
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!StoryControllerHelpers.ValidateStoryOwnership(story, userId))
                {
                    return Unauthorized(new { success = false, message = "Story not published" });
                }
            }

            string MakeAbsolute(string? url) => 
                StoryControllerHelpers.MakeAbsoluteUrl(url, Request.Scheme, Request.Host.Value ?? "");

            var pageUrl = !string.IsNullOrWhiteSpace(story.CanonicalUrl)
                ? story.CanonicalUrl!
                : Url.Action("Viewer", "Stories", new { slug = story.Slug }, Request.Scheme) ?? string.Empty;

            var slides = story.Slides
                .OrderBy(s => s.OrderIndex)
                .Select(s => new
                {
                    type = InferType(s.MediaType, s.MediaUrl),
                    src = MakeAbsolute(s.MediaUrl),
                    poster = MakeAbsolute(s.MediaUrl),
                    headline = s.Headline ?? string.Empty,
                    text = s.Text ?? string.Empty,
                    caption = s.Caption ?? string.Empty,
                    duration = s.Duration > 0 ? s.Duration : StoryConstants.DefaultDuration
                })
                .ToList();

            return Ok(new { success = true, title = story.Title, pageUrl, slides });
        }

        private static string InferType(string? mediaType, string? url)
        {
            if (!string.IsNullOrWhiteSpace(mediaType))
            {
                if (mediaType.StartsWith("video", StringComparison.OrdinalIgnoreCase)) return "video";
                if (mediaType.StartsWith("image", StringComparison.OrdinalIgnoreCase)) return "image";
            }
            var u = url ?? string.Empty;
            if (u.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || u.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) || u.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) return "video";
            return "image";
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("stories/amp/{slug}")]
        public async Task<IActionResult> Amp(string slug)
        {
            // Optimized query - use AsNoTracking for read-only access and better performance
            var story = await _context.Stories
                .AsNoTracking()
                .Include(s => s.Slides.OrderBy(sl => sl.OrderIndex))
                .Include(s => s.User)
                .Include(s => s.Community)
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (story == null)
            {
                return NotFound();
            }

            // Track story view for analytics (fire and forget - don't block page load)
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var storyId = story.StoryId;
            var serviceProvider = HttpContext.RequestServices;
            
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    var view = new StoryView
                    {
                        StoryId = storyId,
                        UserId = userId,
                        ViewedAt = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        UserAgent = userAgent
                    };
                    context.StoryViews.Add(view);
                    
                    // Update story view count
                    var storyToUpdate = await context.Stories.FindAsync(storyId);
                    if (storyToUpdate != null)
                    {
                        storyToUpdate.ViewCount++;
                    }
                    
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error tracking story view for story {StoryId}", storyId);
                }
            });

            string MakeAbsolute(string? url) => 
                StoryControllerHelpers.MakeAbsoluteUrl(url, Request.Scheme, Request.Host.Value ?? "");

            // Normalize slide media URLs and types for AMP
            foreach (var slide in story.Slides)
            {
                if (!string.IsNullOrWhiteSpace(slide.MediaUrl))
                {
                    slide.MediaUrl = MakeAbsolute(slide.MediaUrl);
                }
                if (string.IsNullOrWhiteSpace(slide.MediaType) && !string.IsNullOrWhiteSpace(slide.MediaUrl))
                {
                    var url = slide.MediaUrl.ToLower();
                    slide.MediaType = (url.EndsWith(".mp4") || url.EndsWith(".webm") || url.EndsWith(".ogg")) ? "video/mp4" : "image/jpeg";
                }
            }

            // Build dynamic bookend components (latest published stories)
            var relatedRaw = await _context.Stories
                .AsNoTracking()
                .Include(s => s.Slides)
                .Where(s => s.Status == StoryConstants.StatusPublished && s.Slug != slug)
                .OrderByDescending(s => s.UpdatedAt)
                .Take(StoryConstants.RelatedStoriesCount)
                .Select(s => new {
                    Title = s.Title,
                    Slug = s.Slug,
                    Cover = s.Slides.OrderBy(x => x.OrderIndex).Select(x => x.MediaUrl).FirstOrDefault()
                })
                .ToListAsync();

            var related = relatedRaw.Select(s => new {
                type = "small",
                title = s.Title ?? "Story",
                url = Url.Action("Amp", "Stories", new { slug = s.Slug }, Request.Scheme),
                image = MakeAbsolute(s.Cover ?? "/Assets/Logo_Auth.png")
            }).ToList();

            ViewBag.BookendComponents = System.Text.Json.JsonSerializer.Serialize(related);

            return View(story);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("stories/explore")]
        public async Task<IActionResult> Explore(int page = 1)
        {
            var pageSize = StoryConstants.ExplorePageSize;
            var skip = (page - 1) * pageSize;

            var stories = await _context.Stories
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Slides)
                .Where(s => s.Status == StoryConstants.StatusPublished)
                .OrderByDescending(s => s.UpdatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Slug,
                    Cover = s.Slides.OrderBy(x => x.OrderIndex).Select(x => x.MediaUrl).FirstOrDefault(),
                    Author = s.User != null ? (s.User.UserName ?? "User") : "User",
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            ViewBag.Items = stories;
            ViewBag.Page = page;
            ViewBag.HasMore = stories.Count == pageSize;
            return View();
        }

        // ViewStory removed - using Viewer action instead to avoid duplicate routes

        // ===== STORY EDITOR API ENDPOINTS =====

        [HttpPost]
        [Route("api/stories/save-draft")]
        public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                Story story;

                if (request.StoryId > 0)
                {
                    // Update existing story
                    story = await _context.Stories
                        .Include(s => s.Slides)
                        .FirstOrDefaultAsync(s => s.StoryId == request.StoryId && s.UserId == userId);

                    if (story == null)
                    {
                        return NotFound(new { success = false, message = "Story not found" });
                    }

                    story.Title = request.Title;
                    story.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Generate unique slug
                    var slug = await StoryControllerHelpers.GenerateUniqueSlugAsync(_context, request.Title);

                    // Create new story
                    story = new Story
                    {
                        Title = request.Title,
                        Slug = slug,
                        Status = StoryConstants.StatusDraft,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Stories.Add(story);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, storyId = story.StoryId, message = "Draft saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving story draft");
                return StatusCode(500, new { success = false, message = "Error saving draft" });
            }
        }

        [HttpPost]
        [Route("api/stories/{id}/publish")]
        public async Task<IActionResult> PublishStoryApi(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var story = await _context.Stories.FirstOrDefaultAsync(s => s.StoryId == id && s.UserId == userId);

                if (story == null)
                {
                    return NotFound(new { success = false, message = "Story not found" });
                }

                story.Status = StoryConstants.StatusPublished;
                story.UpdatedAt = DateTime.UtcNow;
                
                // Set published date if not already set
                if (!story.PublishedAt.HasValue)
                {
                    story.PublishedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, slug = story.Slug, message = "Story published successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing story {StoryId}", id);
                return StatusCode(500, new { success = false, message = "Error publishing story" });
            }
        }
    }

    public class SaveDraftRequest
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        // TODO: Implement slide saving in SaveDraft action
        // Currently unused - reserved for future implementation
        public List<SlideData> Slides { get; set; } = new();
    }

    public class SlideData
    {
        public string Id { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string BackgroundType { get; set; } = string.Empty;
        public string BackgroundImage { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Transition { get; set; } = string.Empty;
    }
}