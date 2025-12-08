using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Repositories
{
    public class ReactionRepository : IReactionRepository
    {
        private readonly ApplicationDbContext _context;

        public ReactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reaction?> GetByIdAsync(int id)
        {
            return await _context.Reactions
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReactionID == id);
        }

        public async Task<Reaction?> GetByUserAndPostAsync(string userId, int postId)
        {
            return await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.PostID == postId);
        }

        public async Task<Reaction?> GetByUserAndCommentAsync(string userId, int commentId)
        {
            return await _context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CommentID == commentId);
        }

        public async Task<IEnumerable<Reaction>> GetByPostIdAsync(int postId)
        {
            return await _context.Reactions
                .Include(r => r.User)
                .Where(r => r.PostID == postId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reaction>> GetByCommentIdAsync(int commentId)
        {
            return await _context.Reactions
                .Include(r => r.User)
                .Where(r => r.CommentID == commentId)
                .ToListAsync();
        }

        public async Task<Reaction> CreateAsync(Reaction reaction)
        {
            reaction.CreatedAt = DateTime.UtcNow;
            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();
            return reaction;
        }

        public async Task DeleteAsync(int id)
        {
            var reaction = await _context.Reactions.FindAsync(id);
            if (reaction != null)
            {
                _context.Reactions.Remove(reaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetReactionCountAsync(int? postId, int? commentId, string reactionType)
        {
            var query = _context.Reactions.AsQueryable();
            
            if (postId.HasValue)
                query = query.Where(r => r.PostID == postId);
            if (commentId.HasValue)
                query = query.Where(r => r.CommentID == commentId);
            if (!string.IsNullOrEmpty(reactionType))
                query = query.Where(r => r.ReactionType == reactionType);
            
            return await query.CountAsync();
        }

        public async Task<bool> UserHasReactedAsync(string userId, int? postId, int? commentId, string reactionType)
        {
            var query = _context.Reactions
                .Where(r => r.UserId == userId && r.ReactionType == reactionType);
            
            if (postId.HasValue)
                query = query.Where(r => r.PostID == postId);
            if (commentId.HasValue)
                query = query.Where(r => r.CommentID == commentId);
            
            return await query.AnyAsync();
        }
    }
}

