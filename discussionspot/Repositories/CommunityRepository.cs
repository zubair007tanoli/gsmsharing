using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class CommunityRepository : EfRepository<Community>, ICommunityRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Community?> GetCommunityBySlugAsync(string slug)
        {
            return await _context.Communities
                .Include(c => c.Creator)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<IEnumerable<Community>> GetCommunitiesByCategoryAsync(int categoryId)
        {
            return await _context.Communities
                .Where(c => c.CategoryId == categoryId && !c.IsDeleted)
                .OrderByDescending(c => c.MemberCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Community>> GetPopularCommunitiesAsync(int count)
        {
            return await _context.Communities
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.MemberCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Community>> GetRecentCommunitiesAsync(int count)
        {
            return await _context.Communities
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Community>> GetTrendingCommunitiesAsync(int count)
        {
            // For trending, we use a combination of recent activity and members
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            return await _context.Communities
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.PostCount)
                .ThenByDescending(c => c.MemberCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Community>> SearchCommunitiesAsync(string searchTerm)
        {
            return await _context.Communities
                .Where(c => (c.Name.Contains(searchTerm) ||
                             c.Title.Contains(searchTerm) ||
                             c.Description.Contains(searchTerm)) &&
                             !c.IsDeleted)
                .OrderByDescending(c => c.MemberCount)
                .ToListAsync();
        }

        public async Task<Community?> GetCommunityWithMembersAsync(int communityId)
        {
            return await _context.Communities
                .Include(c => c.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.CommunityId == communityId && !c.IsDeleted);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Communities
                .AnyAsync(c => c.Slug == slug);
        }

        public async Task UpdateMemberCountAsync(int communityId)
        {
            var community = await _context.Communities.FindAsync(communityId);
            if (community != null)
            {
                community.MemberCount = await _context.CommunityMembers
                    .CountAsync(m => m.CommunityId == communityId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePostCountAsync(int communityId)
        {
            var community = await _context.Communities.FindAsync(communityId);
            if (community != null)
            {
                community.PostCount = await _context.Posts
                    .CountAsync(p => p.CommunityId == communityId && p.Status == "published");
                await _context.SaveChangesAsync();
            }
        }
    }
}