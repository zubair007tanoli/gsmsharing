using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForumController> _logger;

        public ForumController(ApplicationDbContext context, ILogger<ForumController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Forum
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.UsersFourm
                    .Include(f => f.User)
                    .Include(f => f.Replies)
                    .Where(f => f.Publish == 1)
                    .OrderByDescending(f => f.CreationDate);

                var totalCount = await query.CountAsync();
                var forums = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Get categories
                var categories = await _context.ForumCategory
                    .GroupBy(fc => fc.CategoryName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Categories = categories;

                return View(forums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forums");
                return View(new List<ForumThread>());
            }
        }

        // GET: Forum/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var forum = await _context.UsersFourm
                .Include(f => f.User)
                .Include(f => f.Replies)
                    .ThenInclude(r => r.User)
                .Include(f => f.Categories)
                .FirstOrDefaultAsync(f => f.UserFourmID == id && f.Publish == 1);

            if (forum == null)
            {
                return NotFound();
            }

            // Increment view count
            forum.Views = (forum.Views ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Get related forums
            var relatedForums = await _context.UsersFourm
                .Include(f => f.User)
                .Where(f => f.Publish == 1 && f.UserFourmID != id)
                .OrderByDescending(f => f.Views)
                .Take(6)
                .ToListAsync();

            ViewBag.RelatedForums = relatedForums;

            return View(forum);
        }

        // GET: Forum/Category/{categoryName}
        public async Task<IActionResult> Category(string categoryName, int page = 1, int pageSize = 20)
        {
            var forumIds = await _context.ForumCategory
                .Where(fc => fc.CategoryName == categoryName)
                .Select(fc => fc.UserFourmID)
                .ToListAsync();

            var forums = await _context.UsersFourm
                .Include(f => f.User)
                .Include(f => f.Replies)
                .Where(f => f.Publish == 1 && forumIds.Contains(f.UserFourmID))
                .OrderByDescending(f => f.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CategoryName = categoryName;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = forumIds.Count;

            return View("Index", forums);
        }
    }
}

