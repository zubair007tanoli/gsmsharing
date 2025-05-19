using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class CommunityMemberRepository : EfRepository<CommunityMember>, ICommunityMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityMemberRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CommunityMember?> GetMembershipAsync(string userId, int communityId)
        {
            return await _context.CommunityMembers
                .FirstOrDefaultAsync(m => m.UserId == userId && m.CommunityId == communityId);
        }

        public async Task<IEnumerable<CommunityMember>> GetCommunityMembersAsync(int communityId)
        {
            return await _context.CommunityMembers
                .Include(m => m.User)
                .Where(m => m.CommunityId == communityId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommunityMember>> GetUserMembershipsAsync(string userId)
        {
            return await _context.CommunityMembers
                .Include(m => m.Community)
                .Where(m => m.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommunityMember>> GetModeratorsAsync(int communityId)
        {
            return await _context.CommunityMembers
                .Include(m => m.User)
                .Where(m => m.CommunityId == communityId &&
                      (m.Role == "moderator" || m.Role == "admin"))
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(string userId, int communityId)
        {
            return await _context.CommunityMembers
                .AnyAsync(m => m.UserId == userId && m.CommunityId == communityId);
        }

        public async Task<bool> IsModeratorAsync(string userId, int communityId)
        {
            return await _context.CommunityMembers
                .AnyAsync(m => m.UserId == userId && m.CommunityId == communityId &&
                          (m.Role == "moderator" || m.Role == "admin"));
        }

        public async Task<bool> IsAdminAsync(string userId, int communityId)
        {
            return await _context.CommunityMembers
                .AnyAsync(m => m.UserId == userId && m.CommunityId == communityId &&
                          m.Role == "admin");
        }
    }
}