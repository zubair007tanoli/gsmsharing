using gsmsharing.Database;
using gsmsharing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace gsmsharing.Controllers
{
    [Authorize]
    public class BanAppealController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BanAppealController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Check if user is banned
            if (!user.IsBanned)
            {
                TempData["Error"] = "You are not banned. You cannot submit an appeal.";
                return RedirectToAction("Index", "Home");
            }

            // Check if user already has a pending appeal
            var pendingAppeal = await _context.BanAppeals
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == AppealStatus.Pending);

            if (pendingAppeal != null)
            {
                TempData["Info"] = "You already have a pending appeal. Please wait for admin review.";
                return RedirectToAction("MyAppeals");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppealMessage")] BanAppeal appeal)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsBanned)
            {
                TempData["Error"] = "You are not banned. You cannot submit an appeal.";
                return RedirectToAction("Index", "Home");
            }

            // Check if user already has a pending appeal
            var pendingAppeal = await _context.BanAppeals
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == AppealStatus.Pending);

            if (pendingAppeal != null)
            {
                TempData["Info"] = "You already have a pending appeal. Please wait for admin review.";
                return RedirectToAction("MyAppeals");
            }

            if (ModelState.IsValid)
            {
                appeal.UserId = userId;
                appeal.SubmittedAt = DateTime.UtcNow;
                appeal.Status = AppealStatus.Pending;

                _context.Add(appeal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Your appeal has been submitted successfully. We will review it soon.";
                return RedirectToAction("MyAppeals");
            }

            return View(appeal);
        }

        [HttpGet]
        public async Task<IActionResult> MyAppeals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            
            var appeals = await _context.BanAppeals
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            ViewBag.IsBanned = user?.IsBanned ?? false;
            return View(appeals);
        }
    }
}

