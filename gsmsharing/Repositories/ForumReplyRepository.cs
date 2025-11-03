using gsmsharing.Database;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using Microsoft.EntityFrameworkCore;

namespace gsmsharing.Repositories
{
    public class ForumReplyRepository : IForumReplyRepository
    {
        private readonly ApplicationDbContext _context;

        public ForumReplyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumReply>> GetRepliesByThreadIdAsync(int threadId)
        {
            return await _context.ForumReplys
                .Where(r => r.ThreadId == threadId)
                .Include(r => r.User)
                .OrderBy(r => r.PublishDate)
                .ToListAsync();
        }

        public async Task<ForumReply?> GetReplyByIdAsync(int replyId)
        {
            return await _context.ForumReplys
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == replyId);
        }

        public async Task<ForumReply> AddReplyAsync(ForumReply reply)
        {
            await _context.ForumReplys.AddAsync(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<bool> UpdateReplyAsync(ForumReply reply)
        {
            _context.ForumReplys.Update(reply);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteReplyAsync(int replyId)
        {
            var reply = await _context.ForumReplys.FindAsync(replyId);
            if (reply == null) return false;

            _context.ForumReplys.Remove(reply);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}

