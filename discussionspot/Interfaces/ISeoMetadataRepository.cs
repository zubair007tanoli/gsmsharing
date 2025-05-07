using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ISeoMetadataRepository : IRepository<SeoMetadata>
    {
        Task<SeoMetadata?> GetSeoForPostAsync(int postId);
        Task<SeoMetadata?> GetSeoForCommunityAsync(int communityId);
        Task<bool> EntityHasSeoAsync(string entityType, int entityId);
    }
}
