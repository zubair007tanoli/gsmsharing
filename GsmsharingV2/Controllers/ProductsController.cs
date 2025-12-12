using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            try
            {
                var query = _context.AffiliationProducts
                    .Include(ap => ap.User)
                    .OrderByDescending(ap => ap.CreationDate);

                var totalCount = await query.CountAsync();
                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Get featured products for sidebar
                var featuredProducts = await _context.AffiliationProducts
                    .Include(ap => ap.User)
                    .OrderByDescending(ap => ap.Views)
                    .Take(6)
                    .ToListAsync();

                ViewBag.FeaturedProducts = featuredProducts;

                // Get recent reviews
                var recentReviews = await _context.ProductReview
                    .Include(pr => pr.User)
                    .Include(pr => pr.AffiliationProduct)
                    .OrderByDescending(pr => pr.ReviewDate)
                    .Take(5)
                    .ToListAsync();

                ViewBag.RecentReviews = recentReviews;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                ViewBag.ErrorMessage = "Failed to load products. Please try again later.";
                return View(new List<AffiliationProduct>());
            }
        }

        // GET: Products/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.AffiliationProducts
                .Include(ap => ap.User)
                .FirstOrDefaultAsync(ap => ap.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Increment view count
            product.Views = (product.Views ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Get product reviews
            var productReviews = await _context.ProductReview
                .Include(pr => pr.User)
                .Where(pr => pr.BlogId == id)
                .OrderByDescending(pr => pr.ReviewDate)
                .ToListAsync();

            // Get related products
            var relatedProducts = await _context.AffiliationProducts
                .Include(ap => ap.User)
                .Where(ap => ap.ProductId != id)
                .OrderByDescending(ap => ap.Views)
                .Take(6)
                .ToListAsync();

            ViewBag.ProductReviews = productReviews;
            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.ReviewCount = productReviews.Count;

            return View(product);
        }
    }
}

