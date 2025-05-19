using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class PostTagRepository : EfRepository<PostTag>, IPostTagRepository
    {
        private readonly ApplicationDbContext _context;

        public PostTagRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetTagsForPostAsync(int postId)
        {
            return await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Select(pt => pt.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByTagAsync(int tagId, int page, int pageSize)
        {
            return await _context.PostTags
                .Where(pt => pt.TagId == tagId)
                .Select(pt => pt.Post)
                .Where(p => p.Status == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task RemoveAllTagsFromPostAsync(int postId)
        {
            var postTags = await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .ToListAsync();

            _context.PostTags.RemoveRange(postTags);
            await _context.SaveChangesAsync();
        }
    }
}