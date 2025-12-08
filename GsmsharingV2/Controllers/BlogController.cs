using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogController> _logger;

        public BlogController(ApplicationDbContext context, ILogger<BlogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Blog/Blogs
        public async Task<IActionResult> Blogs(int page = 1, int pageSize = 12)
        {
            try
            {
                // Get published blog posts from MobilePosts table (publish is tinyint: 0 or 1)
                var query = _context.MobilePosts
                    .Include(mp => mp.User)
                    .Where(mp => mp.publish == 1)
                    .OrderByDescending(mp => mp.CreationDate);

                var totalCount = await query.CountAsync();
                var blogs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Also get GsmBlog posts
                var gsmBlogs = await _context.GsmBlogs
                    .Include(gb => gb.User)
                    .Where(gb => gb.Publish == true)
                    .OrderByDescending(gb => gb.PublishDate)
                    .Take(5)
                    .ToListAsync();

                ViewBag.GsmBlogs = gsmBlogs;

                // Get affiliate products for sidebar
                var affiliateProducts = await _context.AffiliationProducts
                    .Include(ap => ap.User)
                    .OrderByDescending(ap => ap.Views)
                    .Take(6)
                    .ToListAsync();

                ViewBag.AffiliateProducts = affiliateProducts;

                return View(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading blogs");
                return View(new List<MobilePost>());
            }
        }

        // GET: Blog/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.MobilePosts
                .Include(mp => mp.User)
                .FirstOrDefaultAsync(mp => mp.BlogId == id && mp.publish == 1);

            if (blog == null)
            {
                return NotFound();
            }

            // Increment view count
            blog.views = (blog.views ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Get related blogs
            var relatedBlogs = await _context.MobilePosts
                .Include(mp => mp.User)
                .Where(mp => mp.publish == true && mp.BlogId != id)
                .OrderByDescending(mp => mp.views)
                .Take(6)
                .ToListAsync();

            ViewBag.RelatedBlogs = relatedBlogs;

            return View(blog);
        }

        // GET: Blog/GsmBlog/{id}
        public async Task<IActionResult> GsmBlogDetails(int id)
        {
            var blog = await _context.GsmBlogs
                .Include(gb => gb.User)
                .FirstOrDefaultAsync(gb => gb.BlogId == id && gb.Publish == true);

            if (blog == null)
            {
                return NotFound();
            }

            // Increment view count
            blog.BlogViews = (blog.BlogViews ?? 0) + 1;
            await _context.SaveChangesAsync();

            return View(blog);
        }
    }
}

