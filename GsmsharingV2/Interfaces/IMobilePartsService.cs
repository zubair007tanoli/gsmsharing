using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface IMobilePartsService
    {
        // Ad Operations
        Task<IEnumerable<MobilePartAd>> GetAllAdsAsync(int page = 1, int pageSize = 12);
        Task<MobilePartAd?> GetAdByIdAsync(int id);
        Task<MobilePartAd?> GetAdBySlugAsync(string slug);
        Task<MobilePartAd> CreateAdAsync(MobilePartAd ad, string userId);
        Task<MobilePartAd> UpdateAdAsync(MobilePartAd ad, string userId);
        Task DeleteAdAsync(int id, string userId);
        Task MarkAdAsSoldAsync(int id, string userId);
        Task<MobilePartAd> PromoteAdAsync(int id, string userId);
        
        // Search & Filter
        Task<IEnumerable<MobilePartAd>> SearchAdsAsync(string query);
        Task<IEnumerable<MobilePartAd>> GetAdsByPartTypeAsync(string partType);
        Task<IEnumerable<MobilePartAd>> GetAdsByBrandAsync(string brand);
        Task<IEnumerable<MobilePartAd>> GetAdsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<MobilePartAd>> GetAdsByConditionAsync(string condition);
        Task<IEnumerable<MobilePartAd>> GetAdsByLocationAsync(string location);
        
        // User's Ads
        Task<IEnumerable<MobilePartAd>> GetUserAdsAsync(string userId);
        Task<IEnumerable<MobilePartAd>> GetUserActiveAdsAsync(string userId);
        
        // Featured Ads
        Task<IEnumerable<MobilePartAd>> GetFeaturedAdsAsync(int count = 6);
        Task<IEnumerable<MobilePartAd>> GetRecentAdsAsync(int count = 10);
        
        // Categories
        Task<IEnumerable<MobilePartCategory>> GetAllCategoriesAsync();
        Task<MobilePartCategory?> GetCategoryByIdAsync(int id);
        Task<MobilePartCategory> CreateCategoryAsync(MobilePartCategory category);
        
        // Statistics
        Task<int> GetTotalAdsCountAsync();
        Task<int> GetActiveAdsCountAsync();
    }
}
