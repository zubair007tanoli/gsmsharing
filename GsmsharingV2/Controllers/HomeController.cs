using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GsmsharingV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IPostRepository postRepository, 
            ICategoryRepository categoryRepository,
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _postRepository = postRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Posts Data
                var featuredPosts = await _postRepository.GetFeaturedPostsAsync();
                var recentPosts = await _postRepository.GetRecentPostsAsync(10);
                var categories = await _categoryRepository.GetTopLevelCategoriesAsync();
                
                // Get posts by category
                var postsByCategory = new Dictionary<Category, IEnumerable<Post>>();
                foreach (var category in categories.Take(6))
                {
                    var posts = await _categoryRepository.GetPostsByCategoryIdAsync(category.CategoryID, 5);
                    if (posts.Any())
                    {
                        postsByCategory[category] = posts;
                    }
                }

                // Blog Posts (MobilePosts) - publish is tinyint (0 or 1)
                var recentBlogs = await _context.MobilePosts
                    .Include(mp => mp.User)
                    .Where(mp => mp.publish == 1)
                    .OrderByDescending(mp => mp.CreationDate)
                    .Take(6)
                    .ToListAsync();

                // GsmBlog Posts
                var gsmBlogs = await _context.GsmBlogs
                    .Include(gb => gb.User)
                    .Where(gb => gb.Publish == true)
                    .OrderByDescending(gb => gb.PublishDate)
                    .Take(5)
                    .ToListAsync();

                // Forum Threads - Publish is tinyint (0 or 1)
                var recentForums = await _context.UsersFourm
                    .Include(f => f.User)
                    .Include(f => f.Replies)
                    .Where(f => f.Publish == 1)
                    .OrderByDescending(f => f.CreationDate)
                    .Take(8)
                    .ToListAsync();

                // Forum Activities (Recent Replies)
                var forumActivities = await _context.ForumReplys
                    .Include(fr => fr.User)
                    .Include(fr => fr.Thread)
                    .OrderByDescending(fr => fr.PublishDate)
                    .Take(10)
                    .ToListAsync();

                // Affiliate Products
                var featuredProducts = await _context.AffiliationProducts
                    .Include(ap => ap.User)
                    .OrderByDescending(ap => ap.Views)
                    .Take(6)
                    .ToListAsync();

                // Mobile Ads - Publish is tinyint (0 or 1)
                var mobileAds = await _context.MobileAds
                    .Include(ma => ma.User)
                    .Where(ma => ma.Publish == 1)
                    .OrderByDescending(ma => ma.CreationDate)
                    .Take(4)
                    .ToListAsync();

                // Mobile Specs
                var recentSpecs = await _context.MobileSpecs
                    .Include(ms => ms.User)
                    .OrderByDescending(ms => ms.Launched)
                    .Take(5)
                    .ToListAsync();

                // Communities
                var topCommunities = await _context.Communities
                    .Include(c => c.Creator)
                    .OrderByDescending(c => c.MemberCount)
                    .Take(8)
                    .ToListAsync();

                // Online Users (Users active in last 30 minutes - simulated)
                var onlineUsers = await _context.Users
                    .OrderByDescending(u => u.Id)
                    .Take(12)
                    .ToListAsync();

                // Statistics
                var totalPosts = await _context.Posts.CountAsync();
                var totalBlogs = await _context.MobilePosts.CountAsync(mp => mp.publish == 1);
                var totalForums = await _context.UsersFourm.CountAsync(f => f.Publish == 1);
                var totalUsers = await _context.Users.CountAsync();
                var totalProducts = await _context.AffiliationProducts.CountAsync();
                
                _logger.LogInformation("Homepage data loaded: Blogs={Blogs}, Forums={Forums}, Posts={Posts}, Products={Products}, Users={Users}", 
                    totalBlogs, totalForums, totalPosts, totalProducts, totalUsers);

                // Set ViewBag
                ViewBag.FeaturedPosts = featuredPosts;
                ViewBag.RecentPosts = recentPosts;
                ViewBag.Categories = categories;
                ViewBag.PostsByCategory = postsByCategory;
                ViewBag.RecentBlogs = recentBlogs;
                ViewBag.GsmBlogs = gsmBlogs;
                ViewBag.RecentForums = recentForums;
                ViewBag.ForumActivities = forumActivities;
                ViewBag.FeaturedProducts = featuredProducts;
                ViewBag.MobileAds = mobileAds;
                ViewBag.RecentSpecs = recentSpecs;
                ViewBag.TopCommunities = topCommunities;
                ViewBag.OnlineUsers = onlineUsers;
                ViewBag.TotalPosts = totalPosts;
                ViewBag.TotalBlogs = totalBlogs;
                ViewBag.TotalForums = totalForums;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalProducts = totalProducts;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage data: {Message}", ex.Message);
                // Set empty collections to prevent view errors
                ViewBag.FeaturedPosts = new List<Post>();
                ViewBag.RecentPosts = new List<Post>();
                ViewBag.Categories = new List<Category>();
                ViewBag.PostsByCategory = new Dictionary<Category, IEnumerable<Post>>();
                ViewBag.RecentBlogs = new List<MobilePost>();
                ViewBag.GsmBlogs = new List<GsmBlog>();
                ViewBag.RecentForums = new List<ForumThread>();
                ViewBag.ForumActivities = new List<ForumReply>();
                ViewBag.FeaturedProducts = new List<AffiliationProduct>();
                ViewBag.MobileAds = new List<MobileAd>();
                ViewBag.RecentSpecs = new List<MobileSpecs>();
                ViewBag.TopCommunities = new List<Community>();
                ViewBag.OnlineUsers = new List<ApplicationUser>();
                ViewBag.TotalPosts = 0;
                ViewBag.TotalBlogs = 0;
                ViewBag.TotalForums = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.TotalProducts = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
