using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.ViewComponents
{
    public class EngagingSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EngagingSidebarViewComponent> _logger;

        public EngagingSidebarViewComponent(ApplicationDbContext context, ILogger<EngagingSidebarViewComponent> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new EngagingSidebarViewModel();

            try
            {
                // Random Blog Post (using GsmBlog table - the main blog table with data)
                var blogIds = await _context.GsmBlogs
                    .Where(gb => gb.Publish == true)
                    .Select(gb => gb.BlogId)
                    .ToListAsync();
                if (blogIds.Any())
                {
                    var randomBlogId = blogIds.OrderBy(x => Guid.NewGuid()).First();
                    model.RandomBlog = await _context.GsmBlogs
                        .Include(gb => gb.User)
                        .FirstOrDefaultAsync(gb => gb.BlogId == randomBlogId && gb.Publish == true);
                }

                // Random Forum Thread
                var forumIds = await _context.UsersFourm
                    .Where(f => f.Publish == 1)
                    .Select(f => f.UserFourmID)
                    .ToListAsync();
                if (forumIds.Any())
                {
                    var randomForumId = forumIds.OrderBy(x => Guid.NewGuid()).First();
                    model.RandomForum = await _context.UsersFourm
                        .Include(f => f.User)
                        .FirstOrDefaultAsync(f => f.UserFourmID == randomForumId && f.Publish == 1);
                }

                // Random Product
                var productIds = await _context.AffiliationProducts
                    .Select(ap => ap.ProductId)
                    .ToListAsync();
                if (productIds.Any())
                {
                    var randomProductId = productIds.OrderBy(x => Guid.NewGuid()).First();
                    model.RandomProduct = await _context.AffiliationProducts
                        .Include(ap => ap.User)
                        .FirstOrDefaultAsync(ap => ap.ProductId == randomProductId);
                }

                // Recent Posts (top 3)
                model.RecentPosts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Community)
                    .Where(p => p.PostStatus == "published")
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                // Trending Forums (top 3 by views)
                model.TrendingForums = await _context.UsersFourm
                    .Include(f => f.User)
                    .Where(f => f.Publish == 1)
                    .OrderByDescending(f => f.Views)
                    .Take(3)
                    .ToListAsync();

                // Popular Blogs (top 3 by views) - using GsmBlog table
                model.PopularBlogs = await _context.GsmBlogs
                    .Include(gb => gb.User)
                    .Where(gb => gb.Publish == true)
                    .OrderByDescending(gb => gb.BlogViews)
                    .Take(3)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading engaging sidebar data: {Message}", ex.Message);
            }

            return View(model);
        }
    }

    public class EngagingSidebarViewModel
    {
        public Models.GsmBlog? RandomBlog { get; set; }
        public Models.ForumThread? RandomForum { get; set; }
        public Models.AffiliationProduct? RandomProduct { get; set; }
        public List<Models.Post> RecentPosts { get; set; } = new();
        public List<Models.ForumThread> TrendingForums { get; set; } = new();
        public List<Models.GsmBlog> PopularBlogs { get; set; } = new();
    }
}


