using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Models.ViewModels.HomePage;
using discussionspot9.Models.ViewModels.PollViewModels;
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
        private readonly INotificationService _notificationService;

        public PostService(ApplicationDbContext context, IMemoryCache cache, ILogger<PostService> logger, INotificationService notificationService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _notificationService = notificationService;
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

        // Add these methods to your existing PostService class
        // Update the GetPollDetailsAsync method:
        public async Task<PollViewModel?> GetPollDetailsAsync(int postId, string? userId)
        {
            var post = await _context.Posts
                .Include(p => p.PollOptions)
                .ThenInclude(po => po.Votes)
                .Include(p => p.PollConfiguration) // Add this include
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null || post.PollOptions == null || !post.PollOptions.Any())
                return null;

            var totalVotes = post.PollOptions.Sum(po => po.Votes.Count);
            var hasUserVoted = userId != null && post.PollOptions.Any(po => po.Votes.Any(v => v.UserId == userId));

            return new PollViewModel
            {
                PostId = postId,
                Question = post.Content,
                TotalVotes = totalVotes,
                HasUserVoted = hasUserVoted,
                EndDate = post.PollConfiguration?.EndDate, // Changed from PollExpiresAt
                AllowMultipleChoices = post.PollConfiguration?.AllowMultipleChoices ?? false, // Changed
                Options = post.PollOptions.Select(po => new PollOptionViewModel
                {
                    PollOptionId = po.PollOptionId, // Changed from po.Id
                    OptionText = po.OptionText, // Changed from po.Text
                    VoteCount = po.Votes.Count,
                    HasUserVoted = userId != null && po.Votes.Any(v => v.UserId == userId),
                    VotePercentage = totalVotes > 0 ? (decimal)po.Votes.Count / totalVotes * 100 : 0
                }).OrderBy(o => o.PollOptionId).ToList()
            };
        }

        // Update the GetPostAwardsAsync method:
        public async Task<List<PostAwardViewModel>> GetPostAwardsAsync(int postId)
        {
            var awards = await _context.PostAwards
                .Where(pa => pa.PostId == postId)
                .Include(pa => pa.Award)
                .Include(pa => pa.AwardedByUser) // Changed from GivenByUser
                .Select(pa => new PostAwardViewModel
                {
                    AwardId = pa.AwardId,
                    AwardName = pa.Award.Name,
                    AwardIconUrl = pa.Award.IconUrl,
                    GivenBy = pa.AwardedByUser.UserName, // Changed from Username
                    DateGiven = pa.AwardedAt, // Changed from DateGiven
                    Message = pa.Message
                })
                .ToListAsync();

            return awards;
        }

        // Update the GiveAwardAsync method:
        public async Task<GiveAwardResult> GiveAwardAsync(int postId, int awardId, string userId, string? message)
        {
            try
            {
                var post = await _context.Posts.FindAsync(postId);
                if (post == null)
                {
                    return new GiveAwardResult { Success = false, Message = "Post not found" };
                }

                var award = await _context.Awards.FindAsync(awardId);
                if (award == null)
                {
                    return new GiveAwardResult { Success = false, Message = "Award not found" };
                }

                var postAward = new PostAward
                {
                    PostId = postId,
                    AwardId = awardId,
                    AwardedByUserId = userId, // Changed from GivenByUserId
                    Message = message,
                    AwardedAt = DateTime.UtcNow, // Changed from DateGiven
                    IsAnonymous = false
                };

                _context.PostAwards.Add(postAward);
                await _context.SaveChangesAsync();

                return new GiveAwardResult
                {
                    Success = true,
                    Message = "Award given successfully",
                    AwardId = postAward.PostAwardId // Changed from Id
                };
            }
            catch (Exception ex)
            {
                return new GiveAwardResult { Success = false, Message = ex.Message };
            }
        }

        private static string GetUserInitials(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "?";

            var parts = userName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return string.Join("", parts.Take(2).Select(p => p[0])).ToUpper();
        }

        public async Task<bool> IsPostSavedByUserAsync(int postId, string userId)
        {
            return await _context.SavedPosts.AnyAsync(sp => sp.PostId == postId && sp.UserId == userId);
        }

        public async Task<SavePostResult> ToggleSavePostAsync(int postId, string userId)
        {
            try
            {
                var existingSavedPost = await _context.SavedPosts.FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == userId);

                if (existingSavedPost != null)
                {
                    // Post is already saved, so unsave it
                    _context.SavedPosts.Remove(existingSavedPost);
                    await _context.SaveChangesAsync();
                    return new SavePostResult
                    {
                        Success = true,
                        IsSaved = false,
                        Message = "Post unsaved successfully"
                    };
                }
                else
                {
                    // Post is not saved, so save it
                    var post = await _context.Posts.FindAsync(postId);
                    if (post == null)
                    {
                        return new SavePostResult
                        {
                            Success = false,
                            Message = "Post not found"
                        };
                    }

                    var newSavedPost = new SavedPost
                    {
                        PostId = postId,
                        UserId = userId,
                        SavedAt = DateTime.UtcNow
                    };
                    _context.SavedPosts.Add(newSavedPost);
                    await _context.SaveChangesAsync();
                    return new SavePostResult
                    {
                        Success = true,
                        IsSaved = true,
                        Message = "Post saved successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling save status for post {PostId}", postId);
                return new SavePostResult
                {
                    Success = false,
                    Message = "Failed to toggle save status for post"
                };
            }
        }

        public async Task<List<TrendingTopicViewModel>> GetTrendingTopicsAsync()
        {
            try
            {
                var trendingPosts = await _context.Posts
                    .Include(p => p.Community)
                    .ThenInclude(c => c!.Category)
                    .Where(p => p.Status == "published" && p.CreatedAt > DateTime.UtcNow.AddDays(-7))
                    .OrderByDescending(p => (p.Score * 2) + (p.CommentCount * 3) + (p.ViewCount * 0.1))
                    .Take(5)
                    .Select(p => new TrendingTopicViewModel
                    {
                        PostId = p.PostId,
                        Title = p.Title,
                        Slug = p.Slug,
                        CategoryName = p.Community!.Name,
                        CategorySlug = p.Community.Slug,
                        ReplyCount = p.CommentCount,
                        TrendingScore = p.Score + p.CommentCount + p.ViewCount,
                        CreatedAt = p.CreatedAt,
                        LastActivity = p.UpdatedAt,
                        IsHot = p.CommentCount > 10 || p.Score > 50
                    })
                    .ToListAsync();

                return trendingPosts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending topics");
                return new List<TrendingTopicViewModel>();
            }
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
                HasPoll = post.HasPoll,
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
            var viewModel = new PostCardViewModel
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
                AuthorDisplayName = "Unknown", // This should be fetched from UserProfile
                AuthorInitials = GetInitials("Unknown"),
                CommunityName = p.Community!.Name,
                CommunitySlug = p.Community.Slug,
                Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
                IsSavedByUser = false // Will be set in the controller
            };

            // Set media URL for different post types
            if (p.PostType == "image" || p.PostType == "video")
            {
                viewModel.MediaUrl = p.Media?.FirstOrDefault()?.Url ?? p.Url;
            }
            else if (p.PostType == "link")
            {
                viewModel.LinkUrl = p.Url;
                if (!string.IsNullOrEmpty(p.Url))
                {
                    try
                    {
                        viewModel.LinkDomain = new Uri(p.Url).Host;
                    }
                    catch
                    {
                        viewModel.LinkDomain = "External Link";
                    }
                }
            }
            else if (p.PostType == "poll" && p.PollOptions?.Any() == true)
            {
                viewModel.PollVoteCount = p.PollOptions.Sum(po => po.Votes?.Count ?? 0);
                viewModel.PollEndsAt = p.PollExpiresAt;
            }

            return viewModel;
        }

        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "??";
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }

        public async Task<VotePollResult> VotePollAsync(int postId, string userId, List<int> optionIds)
        {
            try
            {
                // Remove existing votes for this user on this poll
                var existingVotes = await _context.PollVotes
                    .Where(pv => pv.PollOption.PostId == postId && pv.UserId == userId)
                    .ToListAsync();

                _context.PollVotes.RemoveRange(existingVotes);

                // Add new votes
                var newVotes = optionIds.Select(optionId => new PollVote
                {
                    PollOptionId = optionId,
                    UserId = userId,
                    VotedAt = DateTime.UtcNow
                }).ToList();

                _context.PollVotes.AddRange(newVotes);
                await _context.SaveChangesAsync();

                // Get updated vote counts
                var updatedCounts = await _context.PollOptions
                    .Where(po => po.PostId == postId)
                    .Select(po => new { po.PollOptionId, Count = po.Votes.Count })
                    .ToDictionaryAsync(x => x.PollOptionId, x => x.Count);

                return new VotePollResult
                {
                    Success = true,
                    Message = "Vote recorded successfully",
                    UpdatedVoteCounts = updatedCounts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on poll {PostId}", postId);
                return new VotePollResult
                {
                    Success = false,
                    Message = "An error occurred while recording your vote"
                };
            }
        }

        public async Task<PollViewModel?> GetPollDataAsync(int postId)
        {
            return await GetPollDetailsAsync(postId, null); // Re-use the existing method
        }

        public async Task<bool> HasUserVotedInPollAsync(int postId, string userId)
        {
            return await _context.PollVotes
                .AnyAsync(pv => pv.UserId == userId && pv.PollOption.PostId == postId);
        }

        public async Task<List<int>> GetUserPollVotesAsync(int postId, string userId)
        {
            return await _context.PollVotes
                .Where(pv => pv.UserId == userId && pv.PollOption.PostId == postId)
                .Select(pv => pv.PollOptionId)
                .ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            throw new NotImplementedException();
        }
    }
}
