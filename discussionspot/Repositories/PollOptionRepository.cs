using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class PollOptionRepository : EfRepository<PollOption>, IPollOptionRepository
    {
        private readonly ApplicationDbContext _context;

        public PollOptionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PollOption>> GetOptionsForPostAsync(int postId)
        {
            return await _context.PollOptions
                .Where(o => o.PostId == postId)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PollOption?> GetOptionWithVotesAsync(int pollOptionId)
        {
            return await _context.PollOptions
                .Include(o => o.Votes)
                .FirstOrDefaultAsync(o => o.PollOptionId == pollOptionId);
        }

        public async Task UpdateVoteCountAsync(int pollOptionId)
        {
            var option = await _context.PollOptions.FindAsync(pollOptionId);
            if (option != null)
            {
                option.VoteCount = await _context.PollVotes.CountAsync(v => v.PollOptionId == pollOptionId);
                await _context.SaveChangesAsync();
            }
        }
    }
}