using gsmsharing.Database;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using Microsoft.EntityFrameworkCore;

namespace gsmsharing.Repositories
{
    public class ForumThreadRepository : IForumThreadRepository
    {
        private readonly ApplicationDbContext _context;

        public ForumThreadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumThread>> GetAllThreadsAsync()
        {
            return await _context.UsersFourm
                .Include(t => t.User)
                .Include(t => t.Replies)
                .Include(t => t.Categories)
                .OrderByDescending(t => t.CreationDate)
                .ToListAsync();
        }

        public async Task<ForumThread?> GetThreadByIdAsync(int threadId)
        {
            return await _context.UsersFourm
                .Include(t => t.User)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.User)
                .Include(t => t.Categories)
                .FirstOrDefaultAsync(t => t.UserFourmID == threadId);
        }

        public async Task<IEnumerable<ForumThread>> GetPublishedThreadsAsync()
        {
            return await _context.UsersFourm
                .Where(t => t.Publish == 1)
                .Include(t => t.User)
                .Include(t => t.Replies)
                .OrderByDescending(t => t.CreationDate)
                .ToListAsync();
        }

        public async Task<ForumThread> AddThreadAsync(ForumThread thread)
        {
            await _context.UsersFourm.AddAsync(thread);
            await _context.SaveChangesAsync();
            return thread;
        }

        public async Task<bool> UpdateThreadAsync(ForumThread thread)
        {
            _context.UsersFourm.Update(thread);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteThreadAsync(int threadId)
        {
            var thread = await _context.UsersFourm.FindAsync(threadId);
            if (thread == null) return false;

            _context.UsersFourm.Remove(thread);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task IncrementViewCountAsync(int threadId)
        {
            var thread = await _context.UsersFourm.FindAsync(threadId);
            if (thread != null)
            {
                thread.Views = (thread.Views ?? 0) + 1;
                await _context.SaveChangesAsync();
            }
        }
    }
}

