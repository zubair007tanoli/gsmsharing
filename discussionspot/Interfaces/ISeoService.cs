using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface ISeoService
    {
        Task<SeoMetadataViewModel> GetPostSeoAsync(int postId);
        Task<SeoMetadataViewModel> GetCommunitySeoAsync(int communityId);
        Task<bool> UpdatePostSeoAsync(int postId, SeoMetadataViewModel model);
        Task<bool> UpdateCommunitySeoAsync(int communityId, SeoMetadataViewModel model);
        Task<bool> GeneratePostSeoAsync(int postId);
        Task<bool> GenerateCommunitySeoAsync(int communityId);
        Task<string> GenerateSitemapXmlAsync();
    }
}
