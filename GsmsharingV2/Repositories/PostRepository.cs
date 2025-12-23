using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using NewPost = GsmsharingV2.Models.NewSchema.Post;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly NewApplicationDbContext _context;

        public PostRepository(NewApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            var newPost = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostID == id);
            return newPost != null ? ConvertToOldPost(newPost) : null;
        }

        public async Task<Post?> GetBySlugAsync(string slug)
        {
            try
            {
                var newPost = await _context.Posts
                    .FirstOrDefaultAsync(p => p.Slug == slug);
                return newPost != null ? ConvertToOldPost(newPost) : null;
            }
            catch
            {
                // If tables don't exist yet, return null
                return null;
            }
        }

        public async Task<Post?> GetBySlugAndCommunityAsync(string slug, string communitySlug)
        {
            try
            {
                // First try to find community by slug
                var community = await _context.Communities
                    .FirstOrDefaultAsync(c => c.Slug == communitySlug);
                
                if (community == null)
                    return null;
                
                // Then find post by slug and community ID
                var newPost = await _context.Posts
                    .FirstOrDefaultAsync(p => p.Slug == slug && p.CommunityID == community.CommunityID);
                
                return newPost != null ? ConvertToOldPost(newPost) : null;
            }
            catch
            {
                // If tables don't exist yet, return null
                return null;
            }
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            try
            {
                var newPosts = await _context.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                return newPosts.Select(ConvertToOldPost);
            }
            catch
            {
                // If tables don't exist yet, return empty list
                return Enumerable.Empty<Post>();
            }
        }

        public async Task<IEnumerable<Post>> GetByCommunityIdAsync(int communityId)
        {
            var newPosts = await _context.Posts
                .Where(p => p.CommunityID == communityId && p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return newPosts.Select(ConvertToOldPost);
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(string userId)
        {
            if (!long.TryParse(userId, out var userIdLong))
                return Enumerable.Empty<Post>();
                
            var newPosts = await _context.Posts
                .Where(p => p.UserId == userIdLong)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return newPosts.Select(ConvertToOldPost);
        }

        public async Task<Post> CreateAsync(Post post)
        {
            try
            {
                var newPost = ConvertToNewPost(post);
                newPost.CreatedAt = DateTime.UtcNow;
                newPost.UpdatedAt = DateTime.UtcNow;
                
                // Generate SEO-friendly slug if not provided
                if (string.IsNullOrEmpty(newPost.Slug))
                {
                    newPost.Slug = GenerateSlug(newPost.Title);
                }
                
                // Ensure slug is unique within the community
                newPost.Slug = await EnsureUniqueSlugAsync(newPost.Slug, newPost.CommunityID);

                // Set PublishedAt if status is published
                if (newPost.PostStatus == "published" && newPost.PublishedAt == null)
                {
                    newPost.PublishedAt = DateTime.UtcNow;
                }

                _context.Posts.Add(newPost);
                await _context.SaveChangesAsync();
                return ConvertToOldPost(newPost);
            }
            catch (Exception ex)
            {
                // Log the error and rethrow with more context
                throw new InvalidOperationException($"Error creating post in new database. Make sure the Posts table exists in gsmsharingv4 database. Run db_modernized.sql to create the tables. Original error: {ex.Message}", ex);
            }
        }
        
        private async Task<string> EnsureUniqueSlugAsync(string baseSlug, long? communityId)
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
            var newPost = await _context.Posts.FindAsync((long)post.PostID);
            if (newPost == null)
                throw new KeyNotFoundException($"Post with ID {post.PostID} not found");
            
            // Update properties
            newPost.Title = post.Title;
            newPost.Slug = post.Slug;
            newPost.Content = post.Content;
            newPost.Tags = post.Tags;
            newPost.FeaturedImage = post.FeaturedImage;
            newPost.Excerpt = post.Excerpt;
            newPost.MetaTitle = post.MetaTitle;
            newPost.MetaDescription = post.MetaDescription;
            newPost.OgTitle = post.OgTitle;
            newPost.OgDescription = post.OgDescription;
            newPost.OgImage = post.OgImage;
            newPost.CanonicalUrl = post.CanonicalUrl;
            newPost.FocusKeyword = post.FocusKeyword;
            newPost.SchemaMarkup = post.SchemaMarkup;
            newPost.PostStatus = post.PostStatus;
            newPost.IsPromoted = post.IsPromoted ?? false;
            newPost.IsFeatured = post.IsFeatured ?? false;
            newPost.IsPinned = post.IsPinned;
            newPost.IsLocked = post.IsLocked;
            newPost.AllowComments = post.AllowComments ?? true;
            newPost.ViewCount = post.ViewCount ?? 0;
            newPost.CommunityID = post.CommunityID.HasValue ? (long?)post.CommunityID.Value : null;
            newPost.UpdatedAt = DateTime.UtcNow;
            
            if (post.PublishedAt.HasValue)
                newPost.PublishedAt = post.PublishedAt;
            else if (newPost.PostStatus == "published" && newPost.PublishedAt == null)
                newPost.PublishedAt = DateTime.UtcNow;

            _context.Posts.Update(newPost);
            await _context.SaveChangesAsync();
            return ConvertToOldPost(newPost);
        }

        public async Task DeleteAsync(int id)
        {
            var newPost = await _context.Posts.FindAsync((long)id);
            if (newPost != null)
            {
                _context.Posts.Remove(newPost);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Posts.CountAsync();
        }

        public async Task<IEnumerable<Post>> GetPaginatedAsync(int page, int pageSize)
        {
            var newPosts = await _context.Posts
                .Where(p => p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return newPosts.Select(ConvertToOldPost);
        }

        public async Task<IEnumerable<Post>> GetFeaturedPostsAsync()
        {
            var newPosts = await _context.Posts
                .Where(p => p.IsFeatured == true && p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();
            return newPosts.Select(ConvertToOldPost);
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
        {
            var newPosts = await _context.Posts
                .Where(p => p.PostStatus == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
            return newPosts.Select(ConvertToOldPost);
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var newPost = await _context.Posts.FindAsync((long)id);
            if (newPost != null)
            {
                newPost.ViewCount = newPost.ViewCount + 1;
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

        // Conversion helpers between old and new Post models
        private Post ConvertToOldPost(NewPost newPost)
        {
            return new Post
            {
                PostID = (int)newPost.PostID,
                UserId = newPost.UserId?.ToString() ?? string.Empty,
                Title = newPost.Title,
                Slug = newPost.Slug,
                Tags = newPost.Tags,
                Content = newPost.Content,
                FeaturedImage = newPost.FeaturedImage,
                Excerpt = newPost.Excerpt,
                MetaTitle = newPost.MetaTitle,
                MetaDescription = newPost.MetaDescription,
                OgTitle = newPost.OgTitle,
                OgDescription = newPost.OgDescription,
                OgImage = newPost.OgImage,
                CanonicalUrl = newPost.CanonicalUrl,
                FocusKeyword = newPost.FocusKeyword,
                SchemaMarkup = newPost.SchemaMarkup,
                PostStatus = newPost.PostStatus,
                IsPromoted = newPost.IsPromoted,
                IsFeatured = newPost.IsFeatured,
                IsPinned = newPost.IsPinned,
                IsLocked = newPost.IsLocked,
                AllowComments = newPost.AllowComments,
                ViewCount = newPost.ViewCount,
                Score = newPost.Score,
                CommentCount = newPost.CommentCount,
                UpvoteCount = newPost.UpvoteCount,
                DownvoteCount = newPost.DownvoteCount,
                IsDeleted = newPost.IsDeleted,
                CommunityID = newPost.CommunityID.HasValue ? (int?)newPost.CommunityID.Value : null,
                CreatedAt = newPost.CreatedAt,
                UpdatedAt = newPost.UpdatedAt,
                PublishedAt = newPost.PublishedAt,
                DeletedAt = newPost.DeletedAt
            };
        }

        private NewPost ConvertToNewPost(Post oldPost)
        {
            long? userIdLong = null;
            if (!string.IsNullOrEmpty(oldPost.UserId) && long.TryParse(oldPost.UserId, out var parsedUserId))
            {
                userIdLong = parsedUserId;
            }

            return new NewPost
            {
                PostID = oldPost.PostID > 0 ? (long)oldPost.PostID : 0,
                UserId = userIdLong,
                Title = oldPost.Title,
                Slug = oldPost.Slug,
                Tags = oldPost.Tags,
                Content = oldPost.Content,
                FeaturedImage = oldPost.FeaturedImage,
                Excerpt = oldPost.Excerpt,
                MetaTitle = oldPost.MetaTitle,
                MetaDescription = oldPost.MetaDescription,
                OgTitle = oldPost.OgTitle,
                OgDescription = oldPost.OgDescription,
                OgImage = oldPost.OgImage,
                CanonicalUrl = oldPost.CanonicalUrl,
                FocusKeyword = oldPost.FocusKeyword,
                SchemaMarkup = oldPost.SchemaMarkup,
                PostStatus = oldPost.PostStatus ?? "published",
                IsPromoted = oldPost.IsPromoted ?? false,
                IsFeatured = oldPost.IsFeatured ?? false,
                IsPinned = oldPost.IsPinned,
                IsLocked = oldPost.IsLocked,
                AllowComments = oldPost.AllowComments ?? true,
                ViewCount = oldPost.ViewCount ?? 0,
                Score = oldPost.Score,
                CommentCount = oldPost.CommentCount,
                UpvoteCount = oldPost.UpvoteCount,
                DownvoteCount = oldPost.DownvoteCount,
                IsDeleted = oldPost.IsDeleted,
                CommunityID = oldPost.CommunityID.HasValue ? (long?)oldPost.CommunityID.Value : null,
                CreatedAt = oldPost.CreatedAt,
                UpdatedAt = oldPost.UpdatedAt,
                PublishedAt = oldPost.PublishedAt,
                DeletedAt = oldPost.DeletedAt
            };
        }
    }
}

