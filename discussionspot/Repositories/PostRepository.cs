using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace discussionspot.Repositories
{
    public class PostRepository : EfRepository<Post>, IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Post?> GetPostBySlugAsync(string communitySlug, string postSlug)
        {
            var community = await _context.Communities
                .FirstOrDefaultAsync(c => c.Slug == communitySlug);

            if (community == null)
                return null;

            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .Include(p => p.Media)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.CommunityId == community.CommunityId && p.Slug == postSlug);
        }

        public async Task<Post?> GetPostWithDetailsAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .Include(p => p.Media)
                .Include(p => p.Comments)
                .Include(p => p.Votes)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<IEnumerable<Post>> GetPostsByCommunityAsync(int communityId, int page, int pageSize)
        {
            return await _context.Posts
                .Where(p => p.CommunityId == communityId && p.Status == "published")
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetHotPostsAsync(int page, int pageSize)
        {
            return await _context.Posts
                .Where(p => p.Status == "published")
                .Include(p => p.User)
                .Include(p => p.Community)
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetNewPostsAsync(int page, int pageSize)
        {
            return await _context.Posts
                .Where(p => p.Status == "published")
                .Include(p => p.User)
                .Include(p => p.Community)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdatePostScoreAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                // Simple score calculation: upvotes - downvotes
                post.Score = post.UpvoteCount - post.DownvoteCount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCommentCountAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.CommentCount = await _context.Comments.CountAsync(c => c.PostId == postId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SlugExistsInCommunityAsync(int communityId, string slug)
        {
            return await _context.Posts.AnyAsync(p => p.CommunityId == communityId && p.Slug == slug);
        }

        Task<IEnumerable<Post>> IPostRepository.GetPostsByUserAsync(string userId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Post>> IPostRepository.GetPostsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Post>> IPostRepository.GetPostsByTagAsync(int tagId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Post>> IPostRepository.GetTopPostsAsync(string timeFilter, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Post>> IPostRepository.GetTrendingPostsAsync(int count)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Post>> IPostRepository.SearchPostsAsync(string searchTerm, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        Task IPostRepository.UpdateVoteCountsAsync(int postId)
        {
            throw new NotImplementedException();
        }

        Task IPostRepository.IncrementViewCountAsync(int postId)
        {
            throw new NotImplementedException();
        }
    }
}