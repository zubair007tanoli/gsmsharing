using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class PostVoteRepository : EfRepository<PostVote>, IPostVoteRepository
    {
        private readonly ApplicationDbContext _context;

        public PostVoteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PostVote?> GetUserVoteForPostAsync(string userId, int postId)
        {
            return await _context.PostVotes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == postId);
        }

        public async Task<IEnumerable<PostVote>> GetVotesForPostAsync(int postId)
        {
            return await _context.PostVotes
                .Where(v => v.PostId == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostVote>> GetUserVotesAsync(string userId)
        {
            return await _context.PostVotes
                .Where(v => v.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetUpvoteCountAsync(int postId)
        {
            return await _context.PostVotes
                .CountAsync(v => v.PostId == postId && v.VoteType == 1);
        }

        public async Task<int> GetDownvoteCountAsync(int postId)
        {
            return await _context.PostVotes
                .CountAsync(v => v.PostId == postId && v.VoteType == -1);
        }

        public async Task<bool> HasUserVotedAsync(string userId, int postId)
        {
            return await _context.PostVotes
                .AnyAsync(v => v.UserId == userId && v.PostId == postId);
        }
    }
}