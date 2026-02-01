using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Services
{
    public class MobileSpecsService : IMobileSpecsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MobileSpecsService> _logger;

        public MobileSpecsService(ApplicationDbContext context, ILogger<MobileSpecsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Brand Operations

        public async Task<IEnumerable<MobileBrand>> GetAllBrandsAsync()
        {
            return await _context.Set<MobileBrand>()
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<MobileBrand?> GetBrandByIdAsync(int id)
        {
            return await _context.Set<MobileBrand>()
                .Include(b => b.Models)
                .FirstOrDefaultAsync(b => b.BrandID == id);
        }

        public async Task<MobileBrand?> GetBrandBySlugAsync(string slug)
        {
            return await _context.Set<MobileBrand>()
                .Include(b => b.Models)
                .FirstOrDefaultAsync(b => b.Slug == slug);
        }

        public async Task<MobileBrand> CreateBrandAsync(MobileBrand brand)
        {
            brand.Slug = GenerateSlug(brand.Name);
            brand.CreatedAt = DateTime.UtcNow;
            _context.Set<MobileBrand>().Add(brand);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created new mobile brand: {BrandName}", brand.Name);
            return brand;
        }

        public async Task<MobileBrand> UpdateBrandAsync(MobileBrand brand)
        {
            brand.Slug = GenerateSlug(brand.Name);
            brand.CreatedAt ??= DateTime.UtcNow;
            _context.Set<MobileBrand>().Update(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task DeleteBrandAsync(int id)
        {
            var brand = await _context.Set<MobileBrand>().FindAsync(id);
            if (brand != null)
            {
                _context.Set<MobileBrand>().Remove(brand);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted mobile brand: {BrandID}", id);
            }
        }

        #endregion

        #region Model Operations

        public async Task<IEnumerable<MobileModel>> GetAllModelsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<MobileModel?> GetModelByIdAsync(int id)
        {
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.ModelID == id);
        }

        public async Task<MobileModel?> GetModelBySlugAsync(string slug)
        {
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.Slug == slug);
        }

        public async Task<MobileModel?> GetModelBySlugAndBrandAsync(string brandSlug, string modelSlug)
        {
            var brand = await _context.Set<MobileBrand>()
                .FirstOrDefaultAsync(b => b.Slug == brandSlug);
            
            if (brand == null) return null;

            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.BrandID == brand.BrandID && m.Slug == modelSlug);
        }

        public async Task<IEnumerable<MobileModel>> GetModelsByBrandAsync(int brandId)
        {
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .Where(m => m.BrandID == brandId)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<MobileModel> CreateModelAsync(MobileModel model)
        {
            model.Slug = GenerateSlug(model.Brand != null ? $"{model.Brand.Name} {model.Name}" : model.Name);
            model.CreatedAt = DateTime.UtcNow;
            _context.Set<MobileModel>().Add(model);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created new mobile model: {ModelName}", model.Name);
            return model;
        }

        public async Task<MobileModel> UpdateModelAsync(MobileModel model)
        {
            model.Slug = GenerateSlug(model.Brand != null ? $"{model.Brand.Name} {model.Name}" : model.Name);
            model.UpdatedAt = DateTime.UtcNow;
            _context.Set<MobileModel>().Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task DeleteModelAsync(int id)
        {
            var model = await _context.Set<MobileModel>().FindAsync(id);
            if (model != null)
            {
                _context.Set<MobileModel>().Remove(model);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted mobile model: {ModelID}", id);
            }
        }

        #endregion

        #region Search & Filter

        public async Task<IEnumerable<MobileModel>> SearchModelsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<MobileModel>();

            var lowerQuery = query.ToLower();
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .Where(m => m.Name.ToLower().Contains(lowerQuery) ||
                           m.Brand.Name.ToLower().Contains(lowerQuery) ||
                           (m.OS != null && m.OS.ToLower().Contains(lowerQuery)) ||
                           (m.Chipset != null && m.Chipset.ToLower().Contains(lowerQuery)))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobileModel>> GetModelsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .Where(m => m.Price >= minPrice && m.Price <= maxPrice)
                .OrderBy(m => m.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<MobileModel>> GetModelsByFeatureAsync(string feature)
        {
            var lowerFeature = feature.ToLower();
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .Where(m => m.DisplayType!.ToLower().Contains(lowerFeature) ||
                           m.Chipset!.ToLower().Contains(lowerFeature) ||
                           m.MainCameraFeatures!.ToLower().Contains(lowerFeature) ||
                           m.BatteryType!.ToLower().Contains(lowerFeature))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        #endregion

        #region Comparison

        public async Task<IEnumerable<MobileModel>> GetModelsForComparisonAsync(IEnumerable<int> modelIds)
        {
            return await _context.Set<MobileModel>()
                .Include(m => m.Brand)
                .Where(m => modelIds.Contains(m.ModelID))
                .ToListAsync();
        }

        #endregion

        #region Statistics

        public async Task<int> GetTotalModelsCountAsync()
        {
            return await _context.Set<MobileModel>().CountAsync();
        }

        public async Task<int> GetTotalBrandsCountAsync()
        {
            return await _context.Set<MobileBrand>().CountAsync();
        }

        #endregion

        #region Helper Methods

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Guid.NewGuid().ToString();

            return name.ToLower()
                .Replace(" ", "-")
                .Replace("+", "-plus")
                .Replace("&", "-and")
                .Replace("/", "-")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "")
                .Replace(".", "-")
                .Replace("--", "-")
                .Trim('-');
        }

        #endregion
    }
}
