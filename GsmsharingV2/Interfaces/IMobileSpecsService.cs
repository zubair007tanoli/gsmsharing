using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface IMobileSpecsService
    {
        // Brand Operations
        Task<IEnumerable<MobileBrand>> GetAllBrandsAsync();
        Task<MobileBrand?> GetBrandByIdAsync(int id);
        Task<MobileBrand?> GetBrandBySlugAsync(string slug);
        Task<MobileBrand> CreateBrandAsync(MobileBrand brand);
        Task<MobileBrand> UpdateBrandAsync(MobileBrand brand);
        Task DeleteBrandAsync(int id);
        
        // Model Operations
        Task<IEnumerable<MobileModel>> GetAllModelsAsync(int page = 1, int pageSize = 10);
        Task<MobileModel?> GetModelByIdAsync(int id);
        Task<MobileModel?> GetModelBySlugAsync(string slug);
        Task<MobileModel?> GetModelBySlugAndBrandAsync(string brandSlug, string modelSlug);
        Task<IEnumerable<MobileModel>> GetModelsByBrandAsync(int brandId);
        Task<MobileModel> CreateModelAsync(MobileModel model);
        Task<MobileModel> UpdateModelAsync(MobileModel model);
        Task DeleteModelAsync(int id);
        
        // Search & Filter
        Task<IEnumerable<MobileModel>> SearchModelsAsync(string query);
        Task<IEnumerable<MobileModel>> GetModelsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<MobileModel>> GetModelsByFeatureAsync(string feature);
        
        // Comparison
        Task<IEnumerable<MobileModel>> GetModelsForComparisonAsync(IEnumerable<int> modelIds);
        
        // Statistics
        Task<int> GetTotalModelsCountAsync();
        Task<int> GetTotalBrandsCountAsync();
    }
}
