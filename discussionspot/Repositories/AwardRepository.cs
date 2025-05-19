using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class AwardRepository : EfRepository<Award>, IAwardRepository
    {
        private readonly ApplicationDbContext _context;

        public AwardRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Award>> GetActiveAwardsAsync()
        {
            return await _context.Awards
                .Where(a => a.IsActive)
                .OrderBy(a => a.CoinCost)
                .ToListAsync();
        }

        public async Task<Award?> GetAwardWithDetailsAsync(int awardId)
        {
            return await _context.Awards
                .Include(a => a.PostAwards)
                .Include(a => a.CommentAwards)
                .FirstOrDefaultAsync(a => a.AwardId == awardId);
        }
    }
}