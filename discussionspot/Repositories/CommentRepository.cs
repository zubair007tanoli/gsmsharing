using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class CommentRepository : EfRepository<Comment>, ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Comment?> GetCommentWithDetailsAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Votes)
                .Include(c => c.Awards)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task<IEnumerable<Comment>> GetPostCommentsAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetUserCommentsAsync(string userId, int page, int pageSize)
        {
            return await _context.Comments
                .Include(c => c.Post)
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetTopLevelCommentsAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId && c.ParentCommentId == null && !c.IsDeleted)
                .OrderByDescending(c => c.Score)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentRepliesAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ParentCommentId == commentId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentTreeAsync(int postId)
        {
            var allComments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderByDescending(c => c.Score)
                .ToListAsync();

            var topLevelComments = allComments
                .Where(c => c.ParentCommentId == null)
                .ToList();

            // Set up the hierarchy
            foreach (var comment in topLevelComments)
            {
                comment.ChildComments = GetChildComments(allComments, comment.CommentId);
            }

            return topLevelComments;
        }

        private ICollection<Comment> GetChildComments(List<Comment> allComments, int parentId)
        {
            var children = allComments
                .Where(c => c.ParentCommentId == parentId)
                .ToList();

            foreach (var child in children)
            {
                child.ChildComments = GetChildComments(allComments, child.CommentId);
            }

            return children;
        }

        public async Task UpdateCommentScoreAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                // Simple score calculation: upvotes - downvotes
                comment.Score = comment.UpvoteCount - comment.DownvoteCount;
                await _context.SaveChangesAsync();
            }
        }
    }
}