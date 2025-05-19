using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class PostAwardRepository : EfRepository<PostAward>, IPostAwardRepository
    {
        private readonly ApplicationDbContext _context;

        public PostAwardRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostAward>> GetAwardsForPostAsync(int postId)
        {
            return await _context.PostAwards
                .Include(a => a.Award)
                .Include(a => a.AwardedByUser)
                .Where(a => a.PostId == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostAward>> GetAwardsGivenByUserAsync(string userId)
        {
            return await _context.PostAwards
                .Include(a => a.Award)
                .Include(a => a.Post)
                .Where(a => a.AwardedByUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostAward>> GetAwardsReceivedByUserAsync(string userId)
        {
            return await _context.PostAwards
                .Include(a => a.Award)
                .Include(a => a.AwardedByUser)
                .Where(a => a.Post.UserId == userId)
                .ToListAsync();
        }
    }
}