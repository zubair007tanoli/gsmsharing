using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models.NewSchema;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Repositories
{
    public class ForumPostRepository : IForumPostRepository
    {
        private readonly NewApplicationDbContext _context;

        public ForumPostRepository(NewApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ForumPost?> GetByIdAsync(long id)
        {
            return await _context.ForumPosts
                .Include(fp => fp.Media)
                .Include(fp => fp.Votes)
                .FirstOrDefaultAsync(fp => fp.ForumPostID == id && !fp.IsDeleted);
        }

        public async Task<ForumPost?> GetBySlugAsync(string slug)
        {
            return await _context.ForumPosts
                .Include(fp => fp.Media)
                .Include(fp => fp.Votes)
                .FirstOrDefaultAsync(fp => fp.Slug == slug && !fp.IsDeleted);
        }

        public async Task<IEnumerable<ForumPost>> GetAllAsync()
        {
            return await _context.ForumPosts
                .Where(fp => !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media)
                .OrderByDescending(fp => fp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumPost>> GetByCommunityIdAsync(long communityId)
        {
            return await _context.ForumPosts
                .Where(fp => fp.CommunityID == communityId && !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media)
                .OrderByDescending(fp => fp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumPost>> GetByPostTypeAsync(string postType)
        {
            return await _context.ForumPosts
                .Where(fp => fp.PostType == postType && !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media)
                .OrderByDescending(fp => fp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumPost>> GetByUserIdAsync(long userId)
        {
            return await _context.ForumPosts
                .Where(fp => fp.UserId == userId && !fp.IsDeleted)
                .Include(fp => fp.Media)
                .OrderByDescending(fp => fp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumPost>> GetHotPostsAsync(int count = 20)
        {
            // Hot algorithm: Score with time decay
            // Formula: Score / (Hours since creation + 2)^1.5
            var posts = await _context.ForumPosts
                .Where(fp => !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media)
                .ToListAsync();

            var hotPosts = posts
                .Select(fp =>
                {
                    var hoursSinceCreation = (DateTime.UtcNow - (fp.CreatedAt ?? DateTime.UtcNow)).TotalHours;
                    var hotScore = fp.Score / Math.Pow(hoursSinceCreation + 2, 1.5);
                    return new { Post = fp, HotScore = hotScore };
                })
                .OrderByDescending(x => x.HotScore)
                .Take(count)
                .Select(x => x.Post)
                .ToList();

            return hotPosts;
        }

        public async Task<IEnumerable<ForumPost>> GetTopPostsAsync(int count = 20, string timeframe = "all")
        {
            IQueryable<ForumPost> query = _context.ForumPosts
                .Where(fp => !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media);

            // Apply timeframe filter
            if (timeframe != "all")
            {
                var cutoffDate = timeframe switch
                {
                    "day" => DateTime.UtcNow.AddDays(-1),
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddDays(-30),
                    "year" => DateTime.UtcNow.AddDays(-365),
                    _ => DateTime.UtcNow.AddDays(-1)
                };
                query = query.Where(fp => fp.CreatedAt >= cutoffDate);
            }

            return await query
                .OrderByDescending(fp => fp.Score)
                .ThenByDescending(fp => fp.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumPost>> GetNewPostsAsync(int count = 20)
        {
            return await _context.ForumPosts
                .Where(fp => !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media)
                .OrderByDescending(fp => fp.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumPost>> GetRisingPostsAsync(int count = 20)
        {
            // Rising: Posts with recent activity (votes in last hour)
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            
            var posts = await _context.ForumPosts
                .Where(fp => !fp.IsDeleted && fp.PostStatus == "published" && fp.CreatedAt >= oneHourAgo)
                .Include(fp => fp.Media)
                .Include(fp => fp.Votes.Where(v => v.CreatedAt >= oneHourAgo))
                .ToListAsync();

            return posts
                .OrderByDescending(fp => fp.Votes.Count)
                .ThenByDescending(fp => fp.Score)
                .Take(count)
                .ToList();
        }

        public async Task<IEnumerable<ForumPost>> GetPaginatedAsync(int page, int pageSize)
        {
            return await _context.ForumPosts
                .Where(fp => !fp.IsDeleted && fp.PostStatus == "published")
                .Include(fp => fp.Media)
                .OrderByDescending(fp => fp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.ForumPosts
                .CountAsync(fp => !fp.IsDeleted && fp.PostStatus == "published");
        }

        public async Task<ForumPost> CreateAsync(ForumPost post)
        {
            post.CreatedAt = DateTime.UtcNow;
            post.Score = 0;
            post.UpvoteCount = 0;
            post.DownvoteCount = 0;
            post.ViewCount = 0;
            post.CommentCount = 0;

            // Generate slug if not provided
            if (string.IsNullOrEmpty(post.Slug))
            {
                post.Slug = GenerateSlug(post.Title);
            }

            // Ensure slug is unique
            post.Slug = await EnsureUniqueSlugAsync(post.Slug, post.CommunityID);

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<ForumPost> UpdateAsync(ForumPost post)
        {
            post.UpdatedAt = DateTime.UtcNow;
            _context.ForumPosts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task DeleteAsync(long id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null)
            {
                post.IsDeleted = true;
                post.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task IncrementViewCountAsync(long id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task VoteAsync(long postId, long userId, string voteType)
        {
            // Remove existing vote if any
            var existingVote = await _context.ForumPostVotes
                .FirstOrDefaultAsync(v => v.ForumPostID == postId && v.UserId == userId);

            if (existingVote != null)
            {
                // Remove old vote
                var post = await _context.ForumPosts.FindAsync(postId);
                if (post != null)
                {
                    if (existingVote.VoteType == "upvote")
                    {
                        post.UpvoteCount = Math.Max(0, post.UpvoteCount - 1);
                    }
                    else if (existingVote.VoteType == "downvote")
                    {
                        post.DownvoteCount = Math.Max(0, post.DownvoteCount - 1);
                    }
                }
                _context.ForumPostVotes.Remove(existingVote);
            }

            // Add new vote if voteType is provided
            if (!string.IsNullOrEmpty(voteType) && (voteType == "upvote" || voteType == "downvote"))
            {
                var vote = new ForumPostVote
                {
                    ForumPostID = postId,
                    UserId = userId,
                    VoteType = voteType,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ForumPostVotes.Add(vote);

                // Update post vote counts
                var post = await _context.ForumPosts.FindAsync(postId);
                if (post != null)
                {
                    if (voteType == "upvote")
                    {
                        post.UpvoteCount++;
                    }
                    else if (voteType == "downvote")
                    {
                        post.DownvoteCount++;
                    }
                }
            }

            // Update score
            await UpdateScoreAsync(postId);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveVoteAsync(long postId, long userId)
        {
            var vote = await _context.ForumPostVotes
                .FirstOrDefaultAsync(v => v.ForumPostID == postId && v.UserId == userId);

            if (vote != null)
            {
                var post = await _context.ForumPosts.FindAsync(postId);
                if (post != null)
                {
                    if (vote.VoteType == "upvote")
                    {
                        post.UpvoteCount = Math.Max(0, post.UpvoteCount - 1);
                    }
                    else if (vote.VoteType == "downvote")
                    {
                        post.DownvoteCount = Math.Max(0, post.DownvoteCount - 1);
                    }
                }

                _context.ForumPostVotes.Remove(vote);
                await UpdateScoreAsync(postId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateScoreAsync(long postId)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post != null)
            {
                // Score = Upvotes - Downvotes
                post.Score = post.UpvoteCount - post.DownvoteCount;
                await _context.SaveChangesAsync();
            }
        }

        // Helper methods
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            return title
                .ToLower()
                .Trim()
                .Replace(" ", "-")
                .Replace("--", "-")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("&", "")
                .Replace("*", "")
                .Replace("+", "")
                .Replace("=", "")
                .Replace("|", "")
                .Replace("\\", "")
                .Replace("/", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("~", "")
                .Replace("`", "")
                .Substring(0, Math.Min(100, title.Length));
        }

        private async Task<string> EnsureUniqueSlugAsync(string slug, long? communityId)
        {
            var baseSlug = slug;
            var counter = 1;
            var uniqueSlug = slug;

            while (await _context.ForumPosts.AnyAsync(fp => 
                fp.Slug == uniqueSlug && 
                fp.CommunityID == communityId && 
                !fp.IsDeleted))
            {
                uniqueSlug = $"{baseSlug}-{counter}";
                counter++;
            }

            return uniqueSlug;
        }
    }
}





















