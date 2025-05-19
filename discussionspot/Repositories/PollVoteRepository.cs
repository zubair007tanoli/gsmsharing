using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class PollVoteRepository : EfRepository<PollVote>, IPollVoteRepository
    {
        private readonly ApplicationDbContext _context;

        public PollVoteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PollVote>> GetVotesForOptionAsync(int pollOptionId)
        {
            return await _context.PollVotes
                .Where(v => v.PollOptionId == pollOptionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PollVote>> GetUserVotesForPostAsync(string userId, int postId)
        {
            return await _context.PollVotes
                .Where(v => v.UserId == userId && v.PollOption.PostId == postId)
                .ToListAsync();
        }

        public async Task<bool> HasUserVotedOnPollAsync(string userId, int postId)
        {
            return await _context.PollVotes
                .AnyAsync(v => v.UserId == userId && v.PollOption.PostId == postId);
        }

        public async Task<IEnumerable<int>> GetUserVotedOptionsAsync(string userId, int postId)
        {
            return await _context.PollVotes
                .Where(v => v.UserId == userId && v.PollOption.PostId == postId)
                .Select(v => v.PollOptionId)
                .ToListAsync();
        }
    }
}