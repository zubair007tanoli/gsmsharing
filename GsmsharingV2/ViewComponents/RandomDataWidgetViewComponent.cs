using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.ViewComponents
{
    public class RandomDataWidgetViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RandomDataWidgetViewComponent> _logger;

        public RandomDataWidgetViewComponent(ApplicationDbContext context, ILogger<RandomDataWidgetViewComponent> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new RandomDataViewModel();

            // Random Blog Post - get random ID then fetch full entity
            try
            {
                var blogIds = await _context.MobilePosts
                    .Where(mp => mp.publish == 1)
                    .Select(mp => mp.BlogId)
                    .ToListAsync();

                if (blogIds.Any())
                {
                    var randomBlogId = blogIds.OrderBy(x => Guid.NewGuid()).First();
                    model.RandomBlog = await _context.MobilePosts
                        .Include(mp => mp.User)
                        .FirstOrDefaultAsync(mp => mp.BlogId == randomBlogId && mp.publish == 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading random blog in RandomDataWidget: {Message}", ex.Message);
            }

            // Random Product - get random ID then fetch full entity
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading random product in RandomDataWidget: {Message}", ex.Message);
            }

            // Random Forum Thread - get random ID then fetch full entity
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading random forum in RandomDataWidget: {Message}", ex.Message);
            }

            // Random Stats
            try
            {
                model.TotalPosts = await _context.Posts.CountAsync();
                model.TotalBlogs = await _context.MobilePosts.CountAsync(mp => mp.publish == 1);
                model.TotalUsers = await _context.Set<ApplicationUser>().CountAsync();
                model.TotalComments = await _context.Comments.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading stats in RandomDataWidget");
            }

            return View(model);
        }
    }

    public class RandomDataViewModel
    {
        public Models.MobilePost? RandomBlog { get; set; }
        public Models.AffiliationProduct? RandomProduct { get; set; }
        public Models.ForumThread? RandomForum { get; set; }
        public int TotalPosts { get; set; }
        public int TotalBlogs { get; set; }
        public int TotalUsers { get; set; }
        public int TotalComments { get; set; }
    }
}

