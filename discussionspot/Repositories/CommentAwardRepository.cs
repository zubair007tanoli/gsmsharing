using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class CommentAwardRepository : EfRepository<CommentAward>, ICommentAwardRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentAwardRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentAward>> GetAwardsForCommentAsync(int commentId)
        {
            return await _context.CommentAwards
                .Include(a => a.Award)
                .Include(a => a.AwardedByUser)
                .Where(a => a.CommentId == commentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommentAward>> GetCommentAwardsGivenByUserAsync(string userId)
        {
            return await _context.CommentAwards
                .Include(a => a.Award)
                .Include(a => a.Comment)
                .Where(a => a.AwardedByUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommentAward>> GetCommentAwardsReceivedByUserAsync(string userId)
        {
            return await _context.CommentAwards
                .Include(a => a.Award)
                .Include(a => a.AwardedByUser)
                .Where(a => a.Comment.UserId == userId)
                .ToListAsync();
        }
    }
}