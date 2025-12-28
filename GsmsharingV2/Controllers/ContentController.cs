using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GsmsharingV2.Controllers
{
    [Authorize]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContentController> _logger;

        public ContentController(ApplicationDbContext context, ILogger<ContentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Content/Create - Tabbed interface for creating different content types
        public IActionResult Create()
        {
            ViewBag.ContentTypes = new List<string> { "Blog Post", "Mobile Ad", "Mobile Part Ad", "Product", "Gsm Blog" };
            return View();
        }

        // POST: Content/CreateBlogPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlogPost([Bind("Title,Content,Tags,MetaDis,WebLinks,publish")] MobilePost mobilePost)
        {
            if (ModelState.IsValid)
            {
                mobilePost.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                mobilePost.CreationDate = DateTime.UtcNow;
                mobilePost.views = 0;
                mobilePost.likes = 0;
                mobilePost.dislikes = 0;

                _context.Add(mobilePost);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Blog post created successfully!", redirectUrl = "/Blog/Blogs" });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        // POST: Content/CreateMobileAd
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMobileAd([Bind("Title,Discription,Price,Tags,Publish")] MobileAd mobileAd)
        {
            if (ModelState.IsValid)
            {
                mobileAd.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                mobileAd.CreationDate = DateTime.UtcNow;
                mobileAd.Views = 0;
                mobileAd.Likes = 0;
                mobileAd.Dislikes = 0;

                _context.Add(mobileAd);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Mobile ad created successfully!", redirectUrl = "/MobileAds" });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        // POST: Content/CreateMobilePartAd
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMobilePartAd([Bind("Title,Discription,Price,Tags,Publish")] MobilePartAd mobilePartAd)
        {
            if (ModelState.IsValid)
            {
                mobilePartAd.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                mobilePartAd.CreationDate = DateTime.UtcNow;
                mobilePartAd.Views = 0;
                mobilePartAd.Likes = 0;
                mobilePartAd.Dislikes = 0;

                _context.Add(mobilePartAd);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Mobile part ad created successfully!", redirectUrl = "/MobilePartAds" });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        // POST: Content/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct([Bind("Title,ProductDiscription,Keywords,Content,Price,ImageLink,BuyLink")] AffiliationProduct product)
        {
            if (ModelState.IsValid)
            {
                product.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                product.CreationDate = DateTime.UtcNow;
                product.Views = 0;
                product.Likes = 0;
                product.DisLikes = 0;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Product created successfully!", redirectUrl = "/Products" });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        // POST: Content/CreateGsmBlog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGsmBlog([Bind("BlogTitle,BlogDiscription,BlogKeywords,BlogContent,ThumbNailLink,Publish")] GsmBlog gsmBlog)
        {
            if (ModelState.IsValid)
            {
                gsmBlog.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                gsmBlog.PublishDate = DateTime.UtcNow;
                gsmBlog.BlogViews = 0;
                gsmBlog.BlogLikes = 0;
                gsmBlog.BlogDisLikes = 0;

                _context.Add(gsmBlog);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Gsm blog created successfully!", redirectUrl = "/Blog/Blogs" });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }
    }
}














