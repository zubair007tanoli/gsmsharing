using GsmsharingV2.Database;
using GsmsharingV2.Models.NewSchema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly NewApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(NewApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dashboard Index
        public IActionResult Index()
        {
            var stats = new DashboardStats
            {
                TotalAds = _context.ClassifiedAds.Count(),
                ActiveAds = _context.ClassifiedAds.Count(a => a.Status == "Active"),
                TotalFiles = _context.FileRepositories.Count(),
                ActiveFiles = _context.FileRepositories.Count(f => f.IsActive),
                TotalAffiliateProducts = _context.AffiliateProducts.Count(),
                ActiveAffiliateProducts = _context.AffiliateProducts.Count(p => p.IsActive),
                TotalConversations = _context.ChatConversations.Count(),
                TotalMessages = _context.ChatMessages.Count(),
                TotalClicks = _context.AffiliateClicks.Count()
            };

            return View(stats);
        }

        // Classified Ads Management
        public async Task<IActionResult> Ads(int page = 1, int pageSize = 20)
        {
            var ads = await _context.ClassifiedAds
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = await _context.ClassifiedAds.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(ads);
        }

        public async Task<IActionResult> AdDetails(long id)
        {
            var ad = await _context.ClassifiedAds
                .Include(a => a.Category)
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.AdID == id);

            if (ad == null) return NotFound();
            return View(ad);
        }

        [HttpGet]
        public async Task<IActionResult> AdEdit(long? id)
        {
            if (id == null)
            {
                var newAd = new ClassifiedAd();
                ViewBag.Categories = await _context.AdCategories.ToListAsync();
                return View(newAd);
            }

            var ad = await _context.ClassifiedAds
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.AdID == id);

            if (ad == null) return NotFound();

            ViewBag.Categories = await _context.AdCategories.ToListAsync();
            return View(ad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdEdit(ClassifiedAd ad)
        {
            if (ModelState.IsValid)
            {
                if (ad.AdID == 0)
                {
                    ad.CreatedDate = DateTime.UtcNow;
                    _context.ClassifiedAds.Add(ad);
                }
                else
                {
                    _context.ClassifiedAds.Update(ad);
                }
                await _context.SaveChangesAsync();

                // Log admin action
                await LogAdminAction("AdEdit", $"Ad {ad.AdID} {(ad.AdID == 0 ? "created" : "updated")}");

                return RedirectToAction(nameof(Ads));
            }

            ViewBag.Categories = await _context.AdCategories.ToListAsync();
            return View(ad);
        }

        [HttpPost]
        public async Task<IActionResult> AdDelete(long id)
        {
            var ad = await _context.ClassifiedAds.FindAsync(id);
            if (ad != null)
            {
                _context.ClassifiedAds.Remove(ad);
                await _context.SaveChangesAsync();
                await LogAdminAction("AdDelete", $"Ad {id} deleted");
            }
            return RedirectToAction(nameof(Ads));
        }

        // Ad Categories Management
        public async Task<IActionResult> AdCategories()
        {
            var categories = await _context.AdCategories.ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult AdCategoryEdit(int? id)
        {
            if (id == null) return View(new AdCategory());
            var category = _context.AdCategories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdCategoryEdit(AdCategory category)
        {
            if (ModelState.IsValid)
            {
                if (category.CategoryID == 0)
                {
                    _context.AdCategories.Add(category);
                }
                else
                {
                    _context.AdCategories.Update(category);
                }
                await _context.SaveChangesAsync();
                await LogAdminAction("AdCategoryEdit", $"Category {category.CategoryID} {(category.CategoryID == 0 ? "created" : "updated")}");
                return RedirectToAction(nameof(AdCategories));
            }
            return View(category);
        }

        // File Repository Management
        public async Task<IActionResult> Files(int page = 1, int pageSize = 20)
        {
            var files = await _context.FileRepositories
                .Include(f => f.Category)
                .OrderByDescending(f => f.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = await _context.FileRepositories.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Categories = await _context.FileCategories.ToListAsync();

            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> FileEdit(long? id)
        {
            ViewBag.Categories = await _context.FileCategories.ToListAsync();
            if (id == null) return View(new FileRepository());
            var file = await _context.FileRepositories.FindAsync(id);
            if (file == null) return NotFound();
            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileEdit(FileRepository file)
        {
            if (ModelState.IsValid)
            {
                if (file.FileID == 0)
                {
                    file.CreatedDate = DateTime.UtcNow;
                    _context.FileRepositories.Add(file);
                }
                else
                {
                    _context.FileRepositories.Update(file);
                }
                await _context.SaveChangesAsync();
                await LogAdminAction("FileEdit", $"File {file.FileID} {(file.FileID == 0 ? "created" : "updated")}");
                return RedirectToAction(nameof(Files));
            }
            ViewBag.Categories = await _context.FileCategories.ToListAsync();
            return View(file);
        }

        // Affiliate Products Management
        public async Task<IActionResult> AffiliateProducts(int page = 1, int pageSize = 20)
        {
            var products = await _context.AffiliateProducts
                .Include(p => p.Partner)
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = await _context.AffiliateProducts.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Partners = await _context.AffiliatePartners.ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AffiliateProductEdit(long? id)
        {
            ViewBag.Partners = await _context.AffiliatePartners.ToListAsync();
            if (id == null) return View(new AffiliateProductNew());
            var product = await _context.AffiliateProducts.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AffiliateProductEdit(AffiliateProductNew product)
        {
            if (ModelState.IsValid)
            {
                if (product.ProductID == 0)
                {
                    product.CreatedDate = DateTime.UtcNow;
                    _context.AffiliateProducts.Add(product);
                }
                else
                {
                    _context.AffiliateProducts.Update(product);
                }
                await _context.SaveChangesAsync();
                await LogAdminAction("AffiliateProductEdit", $"Product {product.ProductID} {(product.ProductID == 0 ? "created" : "updated")}");
                return RedirectToAction(nameof(AffiliateProducts));
            }
            ViewBag.Partners = await _context.AffiliatePartners.ToListAsync();
            return View(product);
        }

        // Affiliate Partners Management
        public async Task<IActionResult> AffiliatePartners()
        {
            var partners = await _context.AffiliatePartners.ToListAsync();
            return View(partners);
        }

        [HttpGet]
        public IActionResult AffiliatePartnerEdit(int? id)
        {
            if (id == null) return View(new AffiliatePartner());
            var partner = _context.AffiliatePartners.Find(id);
            if (partner == null) return NotFound();
            return View(partner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AffiliatePartnerEdit(AffiliatePartner partner)
        {
            if (ModelState.IsValid)
            {
                if (partner.PartnerID == 0)
                {
                    _context.AffiliatePartners.Add(partner);
                }
                else
                {
                    _context.AffiliatePartners.Update(partner);
                }
                await _context.SaveChangesAsync();
                await LogAdminAction("AffiliatePartnerEdit", $"Partner {partner.PartnerID} {(partner.PartnerID == 0 ? "created" : "updated")}");
                return RedirectToAction(nameof(AffiliatePartners));
            }
            return View(partner);
        }

        // System Settings
        public async Task<IActionResult> Settings()
        {
            var settings = await _context.SystemSettings.ToListAsync();
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSetting(string key, string value)
        {
            var setting = await _context.SystemSettings.FindAsync(key);
            if (setting == null)
            {
                setting = new SystemSetting { SettingKey = key, SettingValue = value };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
                setting.LastUpdated = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            await LogAdminAction("UpdateSetting", $"Setting {key} updated");
            return RedirectToAction(nameof(Settings));
        }

        // Admin Logs
        public async Task<IActionResult> Logs(int page = 1, int pageSize = 50)
        {
            var logs = await _context.AdminLogs
                .OrderByDescending(l => l.LogDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = await _context.AdminLogs.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(logs);
        }

        // Chat Conversations
        public async Task<IActionResult> Conversations(int page = 1, int pageSize = 20)
        {
            var conversations = await _context.ChatConversations
                .Include(c => c.Ad)
                .OrderByDescending(c => c.LastMessageDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = await _context.ChatConversations.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(conversations);
        }

        // Helper method to log admin actions
        // Note: User ID from old database (string) needs to be handled - storing as 0 for now
        // In production, you'd want to sync users between databases or use a mapping table
        private async Task LogAdminAction(string action, string details)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                long userIdLong = 0; // Default to 0 if conversion fails (old DB uses string IDs)
                if (!string.IsNullOrEmpty(userId) && long.TryParse(userId, out userIdLong))
                {
                    // ID is already a valid long
                }
                // If userId is string (from old DB), we'll use 0 or you can implement a user mapping

                var log = new AdminLog
                {
                    AdminUserID = userIdLong,
                    Action = action,
                    Details = $"{details} | OldDB UserID: {userId ?? "Unknown"}",
                    LogDate = DateTime.UtcNow
                };
                _context.AdminLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging admin action: {Action}", action);
            }
        }

        public class DashboardStats
        {
            public int TotalAds { get; set; }
            public int ActiveAds { get; set; }
            public int TotalFiles { get; set; }
            public int ActiveFiles { get; set; }
            public int TotalAffiliateProducts { get; set; }
            public int ActiveAffiliateProducts { get; set; }
            public int TotalConversations { get; set; }
            public int TotalMessages { get; set; }
            public int TotalClicks { get; set; }
        }
    }
}

