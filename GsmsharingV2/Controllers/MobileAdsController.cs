using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GsmsharingV2.Controllers
{
    [Authorize]
    public class MobileAdsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MobileAdsController> _logger;

        public MobileAdsController(ApplicationDbContext context, ILogger<MobileAdsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: MobileAds
        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            var ads = await _context.MobileAds
                .Include(ma => ma.User)
                .Include(ma => ma.Images)
                .Where(ma => ma.Publish == 1)
                .OrderByDescending(ma => ma.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = await _context.MobileAds.CountAsync(ma => ma.Publish == 1);
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / pageSize);

            return View(ads);
        }

        // GET: MobileAds/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mobileAd = await _context.MobileAds
                .Include(ma => ma.User)
                .Include(ma => ma.Images)
                .FirstOrDefaultAsync(m => m.AdsId == id);

            if (mobileAd == null) return NotFound();

            mobileAd.Views = (mobileAd.Views ?? 0) + 1;
            await _context.SaveChangesAsync();

            return View(mobileAd);
        }

        // GET: MobileAds/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MobileAds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Discription,Price,Tags,Publish")] MobileAd mobileAd)
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
                return RedirectToAction(nameof(Index));
            }
            return View(mobileAd);
        }

        // GET: MobileAds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mobileAd = await _context.MobileAds.FindAsync(id);
            if (mobileAd == null) return NotFound();

            if (mobileAd.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(mobileAd);
        }

        // POST: MobileAds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdsId,UserId,Title,Discription,Price,Tags,Publish")] MobileAd mobileAd)
        {
            if (id != mobileAd.AdsId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mobileAd);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MobileAdExists(mobileAd.AdsId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mobileAd);
        }

        // GET: MobileAds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mobileAd = await _context.MobileAds
                .Include(ma => ma.User)
                .FirstOrDefaultAsync(m => m.AdsId == id);

            if (mobileAd == null) return NotFound();

            if (mobileAd.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(mobileAd);
        }

        // POST: MobileAds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mobileAd = await _context.MobileAds.FindAsync(id);
            if (mobileAd != null)
            {
                _context.MobileAds.Remove(mobileAd);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MobileAdExists(int id)
        {
            return _context.MobileAds.Any(e => e.AdsId == id);
        }
    }
}


























