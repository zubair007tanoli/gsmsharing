using gsmsharing.Database;
using gsmsharing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace gsmsharing.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var pendingAppealsCount = await _context.BanAppeals
                .CountAsync(a => a.Status == AppealStatus.Pending);
            
            ViewBag.PendingAppealsCount = pendingAppealsCount;
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult PostManagement()
        {
            return View();
        }

        public IActionResult CommentsManagement()
        {
            return View();
        }

        public IActionResult CommunitiesManagement()
        {
            return View();
        }

        public IActionResult Categories()
        {
            return View();
        }

        // Ban Appeal Management
        [HttpGet]
        public async Task<IActionResult> BanAppeals()
        {
            var appeals = await _context.BanAppeals
                .Include(a => a.User)
                .Include(a => a.ReviewedBy)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(appeals);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewAppeal(int id)
        {
            var appeal = await _context.BanAppeals
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.BanAppealId == id);

            if (appeal == null)
            {
                return NotFound();
            }

            return View(appeal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAppeal(int id, string adminResponse = "")
        {
            var appeal = await _context.BanAppeals
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.BanAppealId == id);

            if (appeal == null)
            {
                return NotFound();
            }

            if (appeal.Status != AppealStatus.Pending)
            {
                TempData["Error"] = "This appeal has already been reviewed.";
                return RedirectToAction("BanAppeals");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Unban the user
            appeal.User.IsBanned = false;
            appeal.User.BannedAt = null;
            appeal.User.BanReason = null;
            appeal.User.BannedByUserId = null;

            // Update appeal status
            appeal.Status = AppealStatus.Approved;
            appeal.ReviewedAt = DateTime.UtcNow;
            appeal.ReviewedByUserId = userId;
            appeal.AdminResponse = adminResponse;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Appeal approved. User {appeal.User.UserName} has been unbanned.";
            return RedirectToAction("BanAppeals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppeal(int id, string adminResponse)
        {
            var appeal = await _context.BanAppeals
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.BanAppealId == id);

            if (appeal == null)
            {
                return NotFound();
            }

            if (appeal.Status != AppealStatus.Pending)
            {
                TempData["Error"] = "This appeal has already been reviewed.";
                return RedirectToAction("BanAppeals");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update appeal status
            appeal.Status = AppealStatus.Rejected;
            appeal.ReviewedAt = DateTime.UtcNow;
            appeal.ReviewedByUserId = userId;
            appeal.AdminResponse = adminResponse ?? "Appeal rejected.";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appeal has been rejected.";
            return RedirectToAction("BanAppeals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId, string banReason)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            user.IsBanned = true;
            user.BannedAt = DateTime.UtcNow;
            user.BanReason = banReason;
            user.BannedByUserId = currentUserId;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"User {user.UserName} has been banned.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBanned = false;
            user.BannedAt = null;
            user.BanReason = null;
            user.BannedByUserId = null;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"User {user.UserName} has been unbanned.";
            return RedirectToAction("Users");
        }
    }
}
