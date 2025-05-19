using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class SeoMetadataRepository : EfRepository<SeoMetadata>, ISeoMetadataRepository
    {
        private readonly ApplicationDbContext _context;

        public SeoMetadataRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SeoMetadata?> GetSeoForPostAsync(int postId)
        {
            return await _context.SeoMetadata
                .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);
        }

        public async Task<SeoMetadata?> GetSeoForCommunityAsync(int communityId)
        {
            return await _context.SeoMetadata
                .FirstOrDefaultAsync(s => s.EntityType == "community" && s.EntityId == communityId);
        }

        public async Task<bool> EntityHasSeoAsync(string entityType, int entityId)
        {
            return await _context.SeoMetadata
                .AnyAsync(s => s.EntityType == entityType && s.EntityId == entityId);
        }
    }
}