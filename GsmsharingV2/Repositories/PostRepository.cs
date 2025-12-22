using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .FirstOrDefaultAsync(p => p.PostID == id);
        }

        public async Task<Post?> GetBySlugAsync(string slug)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<Post?> GetBySlugAndCommunityAsync(string slug, string communitySlug)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .Where(p => p.Slug == slug && p.Community.Slug == communitySlug)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByCommunityIdAsync(int communityId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Where(p => p.CommunityID == communityId && p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(string userId)
        {
            return await _context.Posts
                .Include(p => p.Community)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post> CreateAsync(Post post)
        {
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;
            
            // Generate SEO-friendly slug if not provided
            if (string.IsNullOrEmpty(post.Slug))
            {
                post.Slug = GenerateSlug(post.Title);
            }
            
            // Ensure slug is unique within the community
            post.Slug = await EnsureUniqueSlugAsync(post.Slug, post.CommunityID);

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }
        
        private async Task<string> EnsureUniqueSlugAsync(string baseSlug, int? communityId)
        {
            if (string.IsNullOrEmpty(baseSlug))
                return baseSlug;
                
            var slug = baseSlug;
            int counter = 1;
            
            while (await _context.Posts
                .AnyAsync(p => p.Slug == slug && 
                              (communityId == null || p.CommunityID == communityId)))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            
            return slug;
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            post.UpdatedAt = DateTime.UtcNow;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task DeleteAsync(int id)
        {
            var post = await GetByIdAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Posts.CountAsync();
        }

        public async Task<IEnumerable<Post>> GetPaginatedAsync(int page, int pageSize)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .Where(p => p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetFeaturedPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .Where(p => p.IsFeatured == true && p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Community)
                .Where(p => p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                post.ViewCount = (post.ViewCount ?? 0) + 1;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Convert to lowercase and trim
            var slug = title.ToLower().Trim();
            
            // Remove special characters, keep only alphanumeric, spaces, and hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            
            // Replace multiple spaces/hyphens with single hyphen
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s_-]+", "-");
            
            // Remove leading/trailing hyphens
            slug = slug.Trim('-');
            
            // Limit length for SEO (max 100 characters)
            if (slug.Length > 100)
            {
                slug = slug.Substring(0, 100).TrimEnd('-');
            }
            
            // Limit length for SEO (max 100 characters)
            if (slug.Length > 100)
            {
                slug = slug.Substring(0, 100).TrimEnd('-');
            }
            
            return slug;
        }
    }
}

