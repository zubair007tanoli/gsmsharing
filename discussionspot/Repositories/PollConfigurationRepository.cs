using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class PollConfigurationRepository : EfRepository<PollConfiguration>, IPollConfigurationRepository
    {
        private readonly ApplicationDbContext _context;

        public PollConfigurationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PollConfiguration?> GetConfigForPostAsync(int postId)
        {
            return await _context.PollConfigurations
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<IEnumerable<PollConfiguration>> GetActivePollsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.PollConfigurations
                .Include(p => p.Post)
                .Where(p => p.EndDate == null || p.EndDate > now)
                .ToListAsync();
        }

        public async Task<IEnumerable<PollConfiguration>> GetExpiredPollsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.PollConfigurations
                .Include(p => p.Post)
                .Where(p => p.EndDate != null && p.EndDate <= now)
                .ToListAsync();
        }
    }
}