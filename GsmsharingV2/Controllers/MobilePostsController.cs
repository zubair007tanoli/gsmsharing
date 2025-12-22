using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GsmsharingV2.Controllers
{
    [Authorize]
    public class MobilePostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MobilePostsController> _logger;

        public MobilePostsController(ApplicationDbContext context, ILogger<MobilePostsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: MobilePosts
        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            var posts = await _context.MobilePosts
                .Include(mp => mp.User)
                .Where(mp => mp.publish == 1)
                .OrderByDescending(mp => mp.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = await _context.MobilePosts.CountAsync(mp => mp.publish == 1);
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / pageSize);

            return View(posts);
        }

        // GET: MobilePosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mobilePost = await _context.MobilePosts
                .Include(mp => mp.User)
                .Include(mp => mp.BlogComments)
                .FirstOrDefaultAsync(m => m.BlogId == id);

            if (mobilePost == null) return NotFound();

            mobilePost.views = (mobilePost.views ?? 0) + 1;
            await _context.SaveChangesAsync();

            return View(mobilePost);
        }

        // GET: MobilePosts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MobilePosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,Tags,MetaDis,WebLinks,publish")] MobilePost mobilePost)
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
                return RedirectToAction(nameof(Index));
            }
            return View(mobilePost);
        }

        // GET: MobilePosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mobilePost = await _context.MobilePosts.FindAsync(id);
            if (mobilePost == null) return NotFound();

            if (mobilePost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(mobilePost);
        }

        // POST: MobilePosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BlogId,UserId,Title,Content,Tags,MetaDis,WebLinks,publish")] MobilePost mobilePost)
        {
            if (id != mobilePost.BlogId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mobilePost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MobilePostExists(mobilePost.BlogId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mobilePost);
        }

        // GET: MobilePosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mobilePost = await _context.MobilePosts
                .Include(mp => mp.User)
                .FirstOrDefaultAsync(m => m.BlogId == id);

            if (mobilePost == null) return NotFound();

            if (mobilePost.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(mobilePost);
        }

        // POST: MobilePosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mobilePost = await _context.MobilePosts.FindAsync(id);
            if (mobilePost != null)
            {
                _context.MobilePosts.Remove(mobilePost);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MobilePostExists(int id)
        {
            return _context.MobilePosts.Any(e => e.BlogId == id);
        }
    }
}










