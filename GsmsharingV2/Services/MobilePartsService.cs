using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Services
{
    public class MobilePartsService : IMobilePartsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MobilePartsService> _logger;

        public MobilePartsService(ApplicationDbContext context, ILogger<MobilePartsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Ad Operations

        public async Task<IEnumerable<MobilePartAd>> GetAllAdsAsync(int page = 1, int pageSize = 12)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => a.AdStatus == "Active" || a.Publish == 1)
                .OrderByDescending(a => a.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<MobilePartAd?> GetAdByIdAsync(int id)
        {
            return await _context.Set<MobilePartAd>()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.MobileAdsId == id);
        }

        public async Task<MobilePartAd?> GetAdBySlugAsync(string slug)
        {
            return await _context.Set<MobilePartAd>()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Slug == slug);
        }

        public async Task<MobilePartAd> CreateAdAsync(MobilePartAd ad, string userId)
        {
            ad.UserId = userId;
            ad.Slug = GenerateSlug(ad.Title);
            ad.CreationDate = DateTime.UtcNow;
            ad.AdStatus = "Active";
            ad.Publish = 1;
            ad.Views = 0;
            ad.Likes = 0;
            ad.Favorites = 0;
            ad.ExpiresAt = DateTime.UtcNow.AddDays(30); // Default 30 days expiry
            
            _context.Set<MobilePartAd>().Add(ad);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created new mobile part ad: {AdID} - {Title}", ad.MobileAdsId, ad.Title);
            return ad;
        }

        public async Task<MobilePartAd> UpdateAdAsync(MobilePartAd ad, string userId)
        {
            var existingAd = await _context.Set<MobilePartAd>().FindAsync(ad.MobileAdsId);
            if (existingAd == null)
                throw new KeyNotFoundException("Ad not found");

            // Verify ownership
            if (existingAd.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own ads");

            // Update fields
            existingAd.Title = ad.Title;
            existingAd.Description = ad.Description;
            existingAd.PartType = ad.PartType;
            existingAd.PartCondition = ad.PartCondition;
            existingAd.BrandCompatibility = ad.BrandCompatibility;
            existingAd.ModelCompatibility = ad.ModelCompatibility;
            existingAd.Price = ad.Price;
            existingAd.Currency = ad.Currency;
            existingAd.Location = ad.Location;
            existingAd.City = ad.City;
            existingAd.Country = ad.Country;
            existingAd.OffersShipping = ad.OffersShipping;
            existingAd.ShippingCost = ad.ShippingCost;
            existingAd.QualityGrade = ad.QualityGrade;
            existingAd.ConditionNotes = ad.ConditionNotes;
            existingAd.HasWarranty = ad.HasWarranty;
            existingAd.WarrantyMonths = ad.WarrantyMonths;
            existingAd.FeaturedImage = ad.FeaturedImage;
            existingAd.IsNegotiable = ad.IsNegotiable;
            existingAd.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated mobile part ad: {AdID}", ad.MobileAdsId);
            return existingAd;
        }

        public async Task DeleteAdAsync(int id, string userId)
        {
            var ad = await _context.Set<MobilePartAd>().FindAsync(id);
            if (ad == null)
                throw new KeyNotFoundException("Ad not found");

            if (ad.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own ads");

            _context.Set<MobilePartAd>().Remove(ad);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted mobile part ad: {AdID}", id);
        }

        public async Task MarkAdAsSoldAsync(int id, string userId)
        {
            var ad = await _context.Set<MobilePartAd>().FindAsync(id);
            if (ad == null)
                throw new KeyNotFoundException("Ad not found");

            if (ad.UserId != userId)
                throw new UnauthorizedAccessException("You can only mark your own ads as sold");

            ad.AdStatus = "Sold";
            ad.SoldAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Marked ad {AdID} as sold", id);
        }

        public async Task<MobilePartAd> PromoteAdAsync(int id, string userId)
        {
            var ad = await _context.Set<MobilePartAd>().FindAsync(id);
            if (ad == null)
                throw new KeyNotFoundException("Ad not found");

            if (ad.UserId != userId)
                throw new UnauthorizedAccessException("You can only promote your own ads");

            ad.IsPromoted = true;
            ad.IsFeatured = true;
            ad.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Promoted ad {AdID}", id);
            return ad;
        }

        #endregion

        #region Search & Filter

        public async Task<IEnumerable<MobilePartAd>> SearchAdsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<MobilePartAd>();

            var lowerQuery = query.ToLower();
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) &&
                           (a.Title.ToLower().Contains(lowerQuery) ||
                            a.Description.ToLower().Contains(lowerQuery) ||
                            a.PartType.ToLower().Contains(lowerQuery) ||
                            a.BrandCompatibility.ToLower().Contains(lowerQuery) ||
                            a.ModelCompatibility.ToLower().Contains(lowerQuery)))
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetAdsByPartTypeAsync(string partType)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) &&
                           a.PartType.ToLower() == partType.ToLower())
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetAdsByBrandAsync(string brand)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) &&
                           a.BrandCompatibility.ToLower().Contains(brand.ToLower()))
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetAdsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) &&
                           a.Price >= minPrice && a.Price <= maxPrice)
                .OrderBy(a => a.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetAdsByConditionAsync(string condition)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) &&
                           a.PartCondition.ToLower() == condition.ToLower())
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetAdsByLocationAsync(string location)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) &&
                           (a.Location.ToLower().Contains(location.ToLower()) ||
                            a.City.ToLower().Contains(location.ToLower()) ||
                            a.Country.ToLower().Contains(location.ToLower())))
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        #endregion

        #region User's Ads

        public async Task<IEnumerable<MobilePartAd>> GetUserAdsAsync(string userId)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetUserActiveAdsAsync(string userId)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => a.UserId == userId && (a.AdStatus == "Active" || a.Publish == 1))
                .OrderByDescending(a => a.CreationDate)
                .ToListAsync();
        }

        #endregion

        #region Featured Ads

        public async Task<IEnumerable<MobilePartAd>> GetFeaturedAdsAsync(int count = 6)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => (a.AdStatus == "Active" || a.Publish == 1) && a.IsFeatured)
                .OrderByDescending(a => a.CreationDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobilePartAd>> GetRecentAdsAsync(int count = 10)
        {
            return await _context.Set<MobilePartAd>()
                .Where(a => a.AdStatus == "Active" || a.Publish == 1)
                .OrderByDescending(a => a.CreationDate)
                .Take(count)
                .ToListAsync();
        }

        #endregion

        #region Categories

        public async Task<IEnumerable<MobilePartCategory>> GetAllCategoriesAsync()
        {
            return await _context.Set<MobilePartCategory>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MobilePartCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.Set<MobilePartCategory>().FindAsync(id);
        }

        public async Task<MobilePartCategory> CreateCategoryAsync(MobilePartCategory category)
        {
            category.Slug = GenerateSlug(category.Name);
            category.CreatedAt = DateTime.UtcNow;
            _context.Set<MobilePartCategory>().Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        #endregion

        #region Statistics

        public async Task<int> GetTotalAdsCountAsync()
        {
            return await _context.Set<MobilePartAd>().CountAsync();
        }

        public async Task<int> GetActiveAdsCountAsync()
        {
            return await _context.Set<MobilePartAd>()
                .CountAsync(a => a.AdStatus == "Active" || a.Publish == 1);
        }

        #endregion

        #region Helper Methods

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Guid.NewGuid().ToString();

            return title.ToLower()
                .Replace(" ", "-")
                .Replace("&", "-and")
                .Replace("/", "-")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "")
                .Replace("--", "-")
                .Trim('-') + "-" + Guid.NewGuid().ToString().Substring(0, 8);
        }

        #endregion
    }
}
