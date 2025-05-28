using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace discussionspot9.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PostService> _logger;

        public PostService(ApplicationDbContext context, IMemoryCache cache, ILogger<PostService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PostListViewModel> GetAllPostsAsync(string sort = "hot", string time = "all", int page = 1)
        {
            const int pageSize = 25;
            var skip = (page - 1) * pageSize;

            var query = _context.Posts
                .Include(p => p.Community)
                .ThenInclude(c => c!.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == "published");

            // Apply time filter for "top" sort
            if (sort == "top" && time != "all")
            {
                var timeFilter = time switch
                {
                    "day" => DateTime.UtcNow.AddDays(-1),
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddDays(-30),
                    "year" => DateTime.UtcNow.AddDays(-365),
                    _ => DateTime.MinValue
                };
                query = query.Where(p => p.CreatedAt >= timeFilter);
            }

            // Get total count before ordering
            var totalPosts = await query.CountAsync();

            // Apply sorting
            List<Post> posts;
            if (sort == "hot")
            {
                // Fetch data first, then apply complex sorting in memory
                posts = await query
                    .OrderByDescending(p => p.Score)
                    .ThenByDescending(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize * 2) // Get extra to account for sorting
                    .ToListAsync();

                // Apply hot algorithm in memory
                var now = DateTime.UtcNow;
                posts = posts
                    .OrderByDescending(p => p.Score / Math.Pow((now - p.CreatedAt).TotalHours + 2, 1.5))
                    .Take(pageSize)
                    .ToList();
            }
            else
            {
                posts = await (sort switch
                {
                    "new" => query.OrderByDescending(p => p.CreatedAt),
                    "top" => query.OrderByDescending(p => p.Score),
                    "controversial" => query.OrderByDescending(p => p.CommentCount)
                        .ThenBy(p => Math.Abs(p.UpvoteCount - p.DownvoteCount)),
                    _ => query.OrderByDescending(p => p.Score).ThenByDescending(p => p.CreatedAt)
                })
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            }

            var postViewModels = posts.Select(p => MapToPostCardViewModel(p)).ToList();

            return new PostListViewModel
            {
                Posts = postViewModels,
                TotalPosts = totalPosts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
                CurrentSort = sort,
                CurrentTimeFilter = time
            };
        }
        public async Task<PostListViewModel> GetCommunityPostsAsync(int communityId, string sort = "hot", int page = 1)
        {
            const int pageSize = 25;
            var skip = (page - 1) * pageSize;

            var query = _context.Posts
                .Include(p => p.Community)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.CommunityId == communityId && p.Status == "published");

            var totalPosts = await query.CountAsync();

            List<Post> posts;
            if (sort == "hot")
            {
                // Fetch pinned posts first, then apply hot algorithm
                var pinnedPosts = await query.Where(p => p.IsPinned).ToListAsync();
                var regularPosts = await query
                    .Where(p => !p.IsPinned)
                    .OrderByDescending(p => p.Score)
                    .ThenByDescending(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize * 2)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var sortedRegularPosts = regularPosts
                    .OrderByDescending(p => p.Score / Math.Pow((now - p.CreatedAt).TotalHours + 2, 1.5))
                    .Take(pageSize - pinnedPosts.Count)
                    .ToList();

                posts = pinnedPosts.Concat(sortedRegularPosts).ToList();
            }
            else
            {
                posts = await (sort switch
                {
                    "new" => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.CreatedAt),
                    "top" => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.Score),
                    _ => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.Score)
                })
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            }

            var postViewModels = posts.Select(p => MapToPostCardViewModel(p)).ToList();

            return new PostListViewModel
            {
                Posts = postViewModels,
                TotalPosts = totalPosts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
                CurrentSort = sort
            };
        }
        public async Task<PostDetailViewModel?> GetPostDetailsAsync(string communitySlug, string postSlug)
        {
            var post = await _context.Posts
                .Include(p => p.Community)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Media)
                .Include(p => p.PollOptions)
                .ThenInclude(po => po.Votes)
                .FirstOrDefaultAsync(p => p.Slug == postSlug &&
                    p.Community!.Slug == communitySlug &&
                    p.Status == "published");

            if (post == null) return null;

            var authorProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == post.UserId);

            return new PostDetailViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                PostType = post.PostType,
                Url = post.Url,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                UpvoteCount = post.UpvoteCount,
                DownvoteCount = post.DownvoteCount,
                CommentCount = post.CommentCount,
                ViewCount = post.ViewCount,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                IsNSFW = post.IsNSFW,
                IsSpoiler = post.IsSpoiler,
                UserId = post.UserId,
                AuthorDisplayName = authorProfile?.DisplayName ?? "Unknown",
                AuthorInitials = GetInitials(authorProfile?.DisplayName ?? "Unknown"),
                AuthorKarma = authorProfile?.KarmaPoints ?? 0,
                CommunityId = post.CommunityId,
                CommunityName = post.Community!.Name,
                CommunitySlug = post.Community.Slug,
                CommunityIconUrl = post.Community.IconUrl,
                Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                Media = post.Media.Select(m => new MediaViewModel
                {
                    MediaId = m.MediaId,
                    Url = m.Url,
                    ThumbnailUrl = m.ThumbnailUrl,
                    MediaType = m.MediaType,
                    Caption = m.Caption
                }).ToList()
            };
        }

        public async Task IncrementViewCountAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int?> GetUserVoteAsync(int postId, string userId)
        {
            var vote = await _context.PostVotes
                .FirstOrDefaultAsync(pv => pv.PostId == postId && pv.UserId == userId);
            return vote?.VoteType;
        }

        public async Task<CreatePostResult> CreatePostAsync(CreatePostViewModel model)
        {
            var slug = model.Title.ToSlug();

            // Ensure unique slug
            var existingCount = await _context.Posts
                .Where(p => p.CommunityId == model.CommunityId && p.Slug.StartsWith(slug))
                .CountAsync();

            if (existingCount > 0)
            {
                slug = $"{slug}-{existingCount + 1}";
            }

            var post = new Post
            {
                Title = model.Title,
                Slug = slug,
                Content = model.Content,
                PostType = model.PostType,
                Url = model.Url,
                UserId = model.UserId,
                CommunityId = model.CommunityId,
                IsNSFW = model.IsNSFW,
                IsSpoiler = model.IsSpoiler,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "published"
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Process tags
            if (!string.IsNullOrEmpty(model.TagsInput))
            {
                var tagNames = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Distinct();

                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            Slug = tagName.ToSlug(),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Tags.Add(tag);
                    }

                    _context.PostTags.Add(new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tag.TagId
                    });
                }
            }

            // Update community post count
            var community = await _context.Communities.FindAsync(model.CommunityId);
            if (community != null)
            {
                community.PostCount++;
            }

            await _context.SaveChangesAsync();
            return new CreatePostResult { Success = true, PostSlug = slug };
        }

        public async Task<VoteResult> VotePostAsync(int postId, string userId, int voteType)
        {
            var existingVote = await _context.PostVotes
                .FirstOrDefaultAsync(pv => pv.PostId == postId && pv.UserId == userId);

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return new VoteResult { Success = false, ErrorMessage = "Post not found" };
            }

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    // Remove vote
                    _context.PostVotes.Remove(existingVote);
                    if (voteType == 1) post.UpvoteCount--;
                    else post.DownvoteCount--;
                    voteType = 0;
                }
                else
                {
                    // Change vote
                    if (existingVote.VoteType == 1) post.UpvoteCount--;
                    else post.DownvoteCount--;

                    existingVote.VoteType = voteType;
                    existingVote.VotedAt = DateTime.UtcNow;

                    if (voteType == 1) post.UpvoteCount++;
                    else post.DownvoteCount++;
                }
            }
            else
            {
                // New vote
                _context.PostVotes.Add(new PostVote
                {
                    PostId = postId,
                    UserId = userId,
                    VoteType = voteType,
                    VotedAt = DateTime.UtcNow
                });

                if (voteType == 1) post.UpvoteCount++;
                else post.DownvoteCount++;
            }

            post.Score = post.UpvoteCount - post.DownvoteCount;
            await _context.SaveChangesAsync();

            return new VoteResult { Success = true, UserVote = voteType == 0 ? null : voteType };
        }

        public async Task<int> GetPostVoteCountAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            return post?.Score ?? 0;
        }

        public async Task IncrementShareCountAsync(int postId)
        {
            // You could track shares in a separate table if needed
            await Task.CompletedTask;
        }

        public async Task<ServiceResult> DeletePostAsync(int postId, string userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return ServiceResult.ErrorResult("Post not found.");
            }

            if (post.UserId != userId)
            {
                // Check if user is moderator
                var isModerator = await _context.CommunityMembers
                    .AnyAsync(cm => cm.UserId == userId &&
                        cm.CommunityId == post.CommunityId &&
                        (cm.Role == "moderator" || cm.Role == "admin"));

                if (!isModerator)
                {
                    return ServiceResult.ErrorResult("You don't have permission to delete this post.");
                }
            }

            post.Status = "deleted";
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult.SuccessResult();
        }

        private static PostCardViewModel MapToPostCardViewModel(Post p)
        {
            // Get author info from UserProfiles
            var authorProfile = p.User != null ?
                new { DisplayName = "Unknown", KarmaPoints = 0 } :
                new { DisplayName = "Unknown", KarmaPoints = 0 };

            return new PostCardViewModel
            {
                PostId = p.PostId,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                PostType = p.PostType,
                Url = p.Url,
                ThumbnailUrl = p.Media?.FirstOrDefault(m => m.MediaType == "image")?.ThumbnailUrl,
                CreatedAt = p.CreatedAt,
                UpvoteCount = p.UpvoteCount,
                DownvoteCount = p.DownvoteCount,
                CommentCount = p.CommentCount,
                ViewCount = p.ViewCount,
                IsPinned = p.IsPinned,
                IsLocked = p.IsLocked,
                IsNSFW = p.IsNSFW,
                IsSpoiler = p.IsSpoiler,
                AuthorDisplayName = authorProfile.DisplayName,
                AuthorInitials = GetInitials(authorProfile.DisplayName),
                CommunityName = p.Community!.Name,
                CommunitySlug = p.Community.Slug,
                Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList()
            };
        }

        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "??";
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }
    }
}