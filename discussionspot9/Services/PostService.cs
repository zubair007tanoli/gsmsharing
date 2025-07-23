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

            // Base query with optimized count retrieval
            var baseQuery = from p in _context.Posts
                            where p.Status == "published"
                            select new
                            {
                                p.PostId,
                                p.CreatedAt,
                                p.ViewCount, // Added view count
                                CommentCount = p.Comments.Count(),
                                UpvoteCount = p.UpvoteCount,
                                DownvoteCount = p.DownvoteCount,
                                Score = p.Votes.Sum(v => (int?)v.VoteType) ?? 0
                            };

            // Apply time filter for 'top' sort
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
                baseQuery = baseQuery.Where(p => p.CreatedAt >= timeFilter);
            }

            var totalPosts = await baseQuery.CountAsync();

            // Get ordered post IDs with counts
            var orderedPostData = await (sort switch
            {
                "new" => baseQuery.OrderByDescending(p => p.CreatedAt),
                "top" => baseQuery.OrderByDescending(p => p.Score),
                "controversial" => baseQuery
                    .OrderByDescending(p => p.CommentCount)
                    .ThenBy(p => Math.Abs(p.UpvoteCount - p.DownvoteCount)),
                "hot" => baseQuery.OrderByDescending(p => p.Score).ThenByDescending(p => p.CreatedAt),
                _ => baseQuery.OrderByDescending(p => p.Score).ThenByDescending(p => p.CreatedAt)
            })
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

            // Apply hot algorithm if needed
            if (sort == "hot")
            {
                var now = DateTime.UtcNow;
                orderedPostData = orderedPostData
                    .OrderByDescending(p => p.Score / Math.Pow((now - p.CreatedAt).TotalHours + 2, 1.5))
                    .ToList();
            }

            // Get full post data with optimized includes
            var postIds = orderedPostData.Select(p => p.PostId).ToList();
            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => postIds.Contains(p.PostId))
                .Select(p => new
                {
                    Post = p,
                    Community = p.Community,
                    Category = p.Community.Category,
                    PostTags = p.PostTags.Select(pt => new { pt.Tag }),
                    UserProfile = p.UserProfile
                })
                .ToListAsync();

            // Maintain order and set counts
            var orderedPosts = postIds.Select(id =>
            {
                var postData = orderedPostData.First(pd => pd.PostId == id);
                var postContainer = posts.First(p => p.Post.PostId == id);

                // Create post with aggregated data
                var post = postContainer.Post;
                post.CommentCount = postData.CommentCount;
                post.UpvoteCount = postData.UpvoteCount;
                post.DownvoteCount = postData.DownvoteCount;
                post.Score = postData.Score;
                post.ViewCount = postData.ViewCount;  // Set view count

                // Attach related entities
                post.Community = postContainer.Community;
                post.Community.Category = postContainer.Category;
                post.PostTags = postContainer.PostTags
                    .Select(pt => new PostTag { Tag = pt.Tag })
                    .ToList();
                post.UserProfile = postContainer.UserProfile;

                return post;
            }).ToList();

            var postViewModels = orderedPosts.Select(p => MapToPostCardViewModel(p)).ToList();

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

            //var query = _context.Posts
            //    .Include(p => p.Community)
            //    .Include(p => p.PostTags)
            //    .ThenInclude(pt => pt.Tag)
            //    .Where(p => p.CommunityId == communityId && p.Status == "published");
            var query = _context.Posts
            .Include(p => p.Community)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.User)
            .Include(p => p.UserProfile)
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

        public async Task<PostDetailViewModel?> GetPostDetailsUpdateAsync(string communitySlug, string postSlug, string? currentUserId = null)
        {
            var post = await _context.Posts
                .Include(p => p.Community)
                    .ThenInclude(c => c.Category) // Include category for CategorySlug
                .Include(p => p.UserProfile) // Include user profile directly
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Media)
                .Include(p => p.PollOptions)
                    .ThenInclude(po => po.Votes)
                .Include(p => p.PollConfiguration) // Add this for poll settings
                .Include(p => p.Awards)
                    .ThenInclude(pa => pa.Award)
                .Include(p => p.Votes) // For checking current user vote
                .FirstOrDefaultAsync(p => p.Slug == postSlug &&
                    p.Community!.Slug == communitySlug &&
                    p.Status == "published");

            if (post == null) return null;

            // Get current user's vote if user is logged in
            int? currentUserVote = null;
            bool isSavedByUser = false;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var userVote = await _context.PostVotes
                    .FirstOrDefaultAsync(pv => pv.PostId == post.PostId && pv.UserId == currentUserId);
                currentUserVote = userVote?.VoteType;

                // Check if post is saved by current user
                isSavedByUser = await _context.SavedPosts
                    .AnyAsync(sp => sp.PostId == post.PostId && sp.UserId == currentUserId);
            }

            // Map poll data if it exists
            var pollViewModel = new PollViewModel();
            if (post.HasPoll && post.PollOptions.Any())
            {
                // Get user's poll votes if logged in
                List<int> userPollVotes = new();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    userPollVotes = await _context.PollVotes
                        .Where(pv => pv.PollOption.PostId == post.PostId && pv.UserId == currentUserId)
                        .Select(pv => pv.PollOptionId)
                        .ToListAsync();
                }

                pollViewModel = new PollViewModel
                {
                    PostId = post.PostId,
                    Question = post.Title, // Poll question is typically the post title, or you might have a separate PollQuestion field
                    Options = post.PollOptions.OrderBy(po => po.DisplayOrder).Select(po => new PollOptionViewModel
                    {
                        PollOptionId = po.PollOptionId,
                        OptionText = po.OptionText,
                        VoteCount = po.VoteCount,
                        VotePercentage = post.PollVoteCount > 0 ? (double)po.VoteCount / post.PollVoteCount * 100 : 0,
                        IsSelected = userPollVotes.Contains(po.PollOptionId) // Mark if user voted for this option
                    }).ToList(),
                    TotalVotes = post.PollVoteCount,

                    // Configuration settings
                    AllowMultipleChoices = post.PollConfiguration?.AllowMultipleChoices ?? false,
                    ShowResultsBeforeVoting = post.PollConfiguration?.ShowResultsBeforeVoting ?? true,
                    ShowResultsBeforeEnd = post.PollConfiguration?.ShowResultsBeforeEnd ?? true,
                    AllowAddingOptions = post.PollConfiguration?.AllowAddingOptions ?? false,

                    // Date and expiration
                    EndDate = post.PollExpiresAt ?? post.PollConfiguration?.EndDate,
                    IsExpired = (post.PollExpiresAt.HasValue && post.PollExpiresAt < DateTime.UtcNow) ||
                               (post.PollConfiguration?.EndDate.HasValue == true && post.PollConfiguration.EndDate < DateTime.UtcNow),

                    // User interaction
                    HasUserVoted = userPollVotes.Any(),
                    UserVotedOptionIds = userPollVotes,
                    UserVotes = userPollVotes // Seems like UserVotes is duplicate of UserVotedOptionIds based on your model
                };
            }

            // Handle link preview data
            var linkModel = new LinkPreviewViewModel();
            if (post.PostType == "link" && !string.IsNullOrEmpty(post.Url))
            {
                linkModel = new LinkPreviewViewModel
                {
                    Url = post.Url,
                    Title = post.LinkPreviewTitle,
                    Description = post.LinkPreviewDescription,
                    ImageUrl = post.LinkPreviewImage,
                    Domain = post.LinkDomain
                };
            }

            return new PostDetailViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                PostType = post.PostType,
                Url = post.Url,
                LinkPreviewImage = post.LinkPreviewImage,
                LinkPreviewDescription = post.LinkPreviewDescription,
                LinkDomain = post.LinkDomain,
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
                IsSavedByUser = isSavedByUser,

                // User info
                UserId = post.UserId,
                AuthorDisplayName = post.UserProfile?.DisplayName ?? "Unknown",
                AuthorInitials = GetInitials(post.UserProfile?.DisplayName ?? "Unknown"),
                AuthorKarma = post.UserProfile?.KarmaPoints ?? 0,
                IsCurrentUserAuthor = post.UserId == currentUserId,

                // Community info
                CommunityId = post.CommunityId,
                CommunityName = post.Community!.Name,
                CommunitySlug = post.Community.Slug,
                CommunityIconUrl = post.Community.IconUrl,
                CategorySlug = post.Community.Category?.Slug,

                // Tags
                Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList(),

                // Media
                Media = post.Media.OrderBy(m => m.DisplayOrder).Select(m => new MediaViewModel
                {
                    MediaId = m.MediaId,
                    Url = m.Url,
                    ThumbnailUrl = m.ThumbnailUrl,
                    MediaType = m.MediaType,
                    Caption = m.Caption,
                    AltText = m.AltText,
                    Width = m.Width,
                    Height = m.Height
                }).ToList(),

                // Poll - Complete mapping
                Poll = pollViewModel,

                // Link preview
                LinkModel = linkModel,

                // Awards
                Awards = post.Awards.Select(pa => new PostAwardViewModel
                {
                    PostAwardId = pa.PostAwardId,
                    AwardName = pa.Award.Name,
                    AwardIconUrl = pa.Award.IconUrl,
                    AwardedAt = pa.AwardedAt,
                    IsAnonymous = pa.IsAnonymous,
                    Message = pa.Message,
                    AwardedByDisplayName = pa.IsAnonymous ? "Anonymous" : pa.AwardedByUser?.DisplayName ?? "Unknown"
                }).ToList(),

                // Current user interaction
                CurrentUserVote = currentUserVote
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

        public async Task<CreatePostResult> CreatePostUpdatedAsync(CreatePostViewModel model)
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

        public async Task<CreatePostResult> CreatePostAsync(CreatePostViewModel model)
        {
            var slug = model.Title.ToSlug();

            // Ensure unique slug within the community
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
                Status = "published",
                // Initialize poll-related fields
                HasPoll = model.PostType == "poll",
                PollOptionCount = model.PostType == "poll" ? (model.PollOptions?.Count ?? 0) : 0,
                PollVoteCount = 0
            };

            // Set poll expiration if it's a poll post
            if (model.PostType == "poll" && model.PollExpiresAt.HasValue)
            {
                post.PollExpiresAt = model.PollExpiresAt.Value;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Save to get the PostId

            // Process poll configuration and options if it's a poll
            if (model.PostType == "poll")
            {
                // Create poll configuration
                var pollConfig = new PollConfiguration
                {
                    PostId = post.PostId,
                    AllowMultipleChoices = model.AllowMultipleChoices ?? false,
                    ShowResultsBeforeVoting = model.ShowResultsBeforeVoting ?? true,
                    ShowResultsBeforeEnd = model.ShowResultsBeforeEnd ?? true,
                    AllowAddingOptions = model.AllowAddingOptions ?? false,
                    MinOptions = model.MinOptions ?? 2,
                    MaxOptions = model.MaxOptions ?? 10,
                    EndDate = model.PollExpiresAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PollConfigurations.Add(pollConfig);

                // Create poll options
                if (model.PollOptions != null && model.PollOptions.Any())
                {
                    for (int i = 0; i < model.PollOptions.Count; i++)
                    {
                        var option = new PollOption
                        {
                            PostId = post.PostId,
                            OptionText = model.PollOptions[i],
                            DisplayOrder = i,
                            VoteCount = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.PollOptions.Add(option);
                    }
                }
            }

            // Process media (images, videos, etc.)
            if (model.MediaUrls != null && model.MediaUrls.Any())
            {
                foreach (var mediaUrl in model.MediaUrls)
                {
                    if (!string.IsNullOrEmpty(mediaUrl))
                    {
                        var media = new Media
                        {
                            PostId = post.PostId,
                            Url = mediaUrl,
                            UserId = model.UserId,
                            MediaType = DetermineMediaType(mediaUrl), // You'll need to implement this method
                            UploadedAt = DateTime.UtcNow,
                            StorageProvider = "external", // or determine based on your setup
                            IsProcessed = true
                        };
                        _context.Media.Add(media);
                    }
                }
            }

            // Process tags
            if (!string.IsNullOrEmpty(model.TagsInput))
            {
                var tagNames = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToList();

                foreach (var tagName in tagNames)
                {
                    // Check if tag already exists
                    var existingTag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName);

                    Tag tag;
                    if (existingTag == null)
                    {
                        // Create new tag
                        tag = new Tag
                        {
                            Name = tagName,
                            Slug = tagName.ToSlug(),
                            CreatedAt = DateTime.UtcNow,
                            PostCount = 1
                        };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync(); // Save to get TagId
                    }
                    else
                    {
                        tag = existingTag;
                        // Increment post count for existing tag
                        tag.PostCount++;
                    }

                    // Create PostTag relationship
                    var postTag = new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tag.TagId
                    };
                    _context.PostTags.Add(postTag);
                }
            }

            // Update community post count
            var community = await _context.Communities.FindAsync(model.CommunityId);
            if (community != null)
            {
                community.PostCount++;
                community.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new CreatePostResult
            {
                Success = true,
                PostSlug = slug,
                PostId = post.PostId
            };
        }

        // Helper method to determine media type from URL
        private string DetermineMediaType(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "document";

            var extension = Path.GetExtension(url).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => "image",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => "video",
                ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" => "audio",
                _ => "document"
            };
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
                AuthorDisplayName = p.UserProfile?.DisplayName ?? "Unknown User",
                AuthorInitials = GetInitials(p.UserProfile?.DisplayName ?? "Unknown"),
                CommunityName = p.Community?.Name ?? "Unknown Community",
                CommunitySlug = p.Community?.Slug ?? "unknown",
                Tags = p.PostTags?.Select(pt => pt.Tag?.Name ?? string.Empty).ToList() ?? new List<string>(), // Fixed CS8619 and IDE0028
                IsSavedByUser = false
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
            var posts = await _context.Posts.FindAsync(postId);
           return posts ?? throw new KeyNotFoundException($"Post with ID {postId} not found.");
        }
    }
}
