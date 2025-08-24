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
using System.Security.Principal;

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

            var baseQuery = from p in _context.Posts
                            where p.Status == "published"
                            select new
                            {
                                p.PostId,
                                p.CreatedAt,
                                p.ViewCount,
                                CommentCount = p.Comments.Count(),
                                UpvoteCount = p.UpvoteCount,
                                DownvoteCount = p.DownvoteCount,
                                Score = p.Votes.Sum(v => (int?)v.VoteType) ?? 0
                            };

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

            var orderedPostData = await (sort switch
            {
                "new" => baseQuery.OrderByDescending(p => p.CreatedAt),
                "top" => baseQuery.OrderByDescending(p => p.Score),
                "controversial" => baseQuery
                    .OrderByDescending(p => p.CommentCount)
                    .ThenBy(p => Math.Abs(p.UpvoteCount - p.DownvoteCount)),
                _ => baseQuery.OrderByDescending(p => p.Score).ThenByDescending(p => p.CreatedAt)
            })
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

            if (sort == "hot")
            {
                var now = DateTime.UtcNow;
                orderedPostData = orderedPostData
                    .OrderByDescending(p => p.Score / Math.Pow((now - p.CreatedAt).TotalHours + 2, 1.5))
                    .ToList();
            }

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

            var orderedPosts = postIds.Select(id =>
            {
                var postData = orderedPostData.First(pd => pd.PostId == id);
                var postContainer = posts.First(p => p.Post.PostId == id);

                var post = postContainer.Post;
                post.CommentCount = postData.CommentCount;
                post.UpvoteCount = postData.UpvoteCount;
                post.DownvoteCount = postData.DownvoteCount;
                post.Score = postData.Score;
                post.ViewCount = postData.ViewCount;

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

        public async Task<PostListViewModel> GetCommunityPostsAsync(int communityId, string sort = "hot", int page = 1)
        {
            const int pageSize = 25;
            var skip = (page - 1) * pageSize;

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
                IsSavedByUser = false,
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
                    Caption = m.Caption,
                    Height = m.Height ?? 0
                }).ToList()
            };
        }

        public async Task<VotePollResult> CastPollVoteAsync(int postId, int pollOptionId, string userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // First check if the post exists and is a poll
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
                if (post == null)
                {
                    _logger.LogError($"Post not found: {postId}");
                    return new VotePollResult { Success = false, Message = "Post not found." };
                }

                if (!post.HasPoll)
                {
                    _logger.LogError($"Post {postId} is not a poll.");
                    return new VotePollResult { Success = false, Message = "This post is not a poll." };
                }

                var existingVote = await _context.PollVotes
                    .FirstOrDefaultAsync(pv => pv.PollOptionId == pollOptionId && pv.UserId == userId);

                var pollConfig = await _context.PollConfigurations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pc => pc.PostId == postId);

                // Create default configuration if missing
                if (pollConfig == null)
                {
                    _logger.LogWarning($"Poll configuration not found for PostId: {postId}. Creating default configuration.");

                    pollConfig = new PollConfiguration
                    {
                        PostId = postId,
                        AllowMultipleChoices = false,
                        ShowResultsBeforeVoting = true,
                        ShowResultsBeforeEnd = true,
                        AllowAddingOptions = false,
                        MinOptions = 2,
                        MaxOptions = 10,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.PollConfigurations.Add(pollConfig);
                    await _context.SaveChangesAsync();
                }

                if (existingVote != null)
                {
                    _context.PollVotes.Remove(existingVote);
                }
                else
                {
                    if (!pollConfig.AllowMultipleChoices)
                    {
                        var existingVotesForPost = await _context.PollVotes
                            .Where(pv => pv.UserId == userId && pv.PollOption.PostId == postId)
                            .ToListAsync();

                        if (existingVotesForPost.Any())
                        {
                            _context.PollVotes.RemoveRange(existingVotesForPost);
                        }
                    }

                    var newVote = new PollVote
                    {
                        PollOptionId = pollOptionId,
                        UserId = userId,
                        VotedAt = DateTime.UtcNow
                    };
                    _context.PollVotes.Add(newVote);
                }

                await _context.SaveChangesAsync();

                var pollOptions = await _context.PollOptions.Where(po => po.PostId == postId).ToListAsync();
                foreach (var option in pollOptions)
                {
                    option.VoteCount = await _context.PollVotes.CountAsync(v => v.PollOptionId == option.PollOptionId);
                }

                if (post != null)
                {
                    post.PollVoteCount = pollOptions.Sum(po => po.VoteCount);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var updatedVoteCounts = pollOptions
                    .ToDictionary(x => x.PollOptionId, x => x.VoteCount);

                return new VotePollResult
                {
                    Success = true,
                    UpdatedVoteCounts = updatedVoteCounts
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error casting poll vote for PostId: {postId}, PollOptionId: {pollOptionId}");
                return new VotePollResult
                {
                    Success = false,
                    Message = "An error occurred while processing your vote."
                };
            }
        }
        public async Task<List<PostAwardViewModel>> GetPostAwardsAsync(int postId)
        {
            return await _context.PostAwards
                .Where(pa => pa.PostId == postId)
                .Include(pa => pa.Award)
                .Include(pa => pa.AwardedByUser)
                .Select(pa => new PostAwardViewModel
                {
                    AwardId = pa.AwardId,
                    AwardName = pa.Award.Name,
                    AwardIconUrl = pa.Award.IconUrl,
                    GivenBy = pa.AwardedByUser.UserName,
                    DateGiven = pa.AwardedAt,
                    Message = pa.Message
                })
                .ToListAsync();
        }

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
                    AwardedByUserId = userId,
                    Message = message,
                    AwardedAt = DateTime.UtcNow,
                    IsAnonymous = false
                };

                _context.PostAwards.Add(postAward);
                await _context.SaveChangesAsync();

                return new GiveAwardResult
                {
                    Success = true,
                    Message = "Award given successfully",
                    AwardId = postAward.PostAwardId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error giving award for post {PostId}", postId);
                return new GiveAwardResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<TrendingTopicViewModel>> GetTrendingTopicsAsync()
        {
            try
            {
                return await _context.Posts
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending topics");
                return new List<TrendingTopicViewModel>();
            }
        }

        public async Task<SavePostResult> ToggleSavePostAsync(int postId, string userId)
        {
            try
            {
                var existingSavedPost = await _context.SavedPosts.FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == userId);

                if (existingSavedPost != null)
                {
                    _context.SavedPosts.Remove(existingSavedPost);
                    await _context.SaveChangesAsync();
                    return new SavePostResult { Success = true, IsSaved = false, Message = "Post unsaved successfully" };
                }
                else
                {
                    var post = await _context.Posts.FindAsync(postId);
                    if (post == null)
                    {
                        return new SavePostResult { Success = false, Message = "Post not found" };
                    }

                    var newSavedPost = new SavedPost { PostId = postId, UserId = userId, SavedAt = DateTime.UtcNow };
                    _context.SavedPosts.Add(newSavedPost);
                    await _context.SaveChangesAsync();
                    return new SavePostResult { Success = true, IsSaved = true, Message = "Post saved successfully" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling save status for post {PostId}", postId);
                return new SavePostResult { Success = false, Message = "Failed to toggle save status for post" };
            }
        }

        public async Task<bool> IsPostSavedByUserAsync(int postId, string userId)
        {
            return await _context.SavedPosts.AnyAsync(sp => sp.PostId == postId && sp.UserId == userId);
        }

        public async Task<CreatePostResult> CreatePostUpdatedAsync(CreatePostViewModel model)
        {
            var slug = model.Title.ToSlug();

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
                Status = !string.IsNullOrEmpty(model.Status) ? model.Status : "published",
                IsPinned = model.IsPinned,
                IsLocked = model.IsLocked,
                HasPoll = model.PostType == "poll",
                PollExpiresAt = model.PollEndDate ?? model.PollExpiresAt,
                PollOptionCount = model.PollOptions?.Count ?? 0,
                PollVoteCount = 0
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

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
                        tag = new Tag { Name = tagName, Slug = tagName.ToSlug(), CreatedAt = DateTime.UtcNow };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }
                    _context.PostTags.Add(new PostTag { PostId = post.PostId, TagId = tag.TagId });
                }
            }

            await SavePollDataAsync(post, model);

            var community = await _context.Communities.FindAsync(model.CommunityId);
            if (community != null)
            {
                community.PostCount++;
            }

            await _context.SaveChangesAsync();

            return new CreatePostResult { Success = true, PostSlug = slug };
        }

        private async Task SavePollDataAsync(Post post, CreatePostViewModel model)
        {
            if (model.PostType != "poll" || model.PollOptions == null || !model.PollOptions.Any(o => !string.IsNullOrWhiteSpace(o)))
            {
                return;
            }

            var pollConfig = new PollConfiguration
            {
                PostId = post.PostId,
                AllowMultipleChoices = model.AllowMultipleChoices,
                ShowResultsBeforeVoting = model.ShowResultsBeforeVoting,
                ShowResultsBeforeEnd = model.ShowResultsBeforeEnd,
                AllowAddingOptions = model.AllowAddingOptions,
                MinOptions = model.MinOptions > 0 ? model.MinOptions : 2,
                MaxOptions = model.MaxOptions > 0 ? model.MaxOptions : 10,
                EndDate = model.PollEndDate ?? model.PollExpiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PollConfigurations.Add(pollConfig);

            for (int i = 0; i < model.PollOptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(model.PollOptions[i]))
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

            // Make sure to save changes
            await _context.SaveChangesAsync();
        }
        // ... (The rest of the file remains unchanged) ...

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

            int? currentUserVote = null;
            bool isSavedByUser = false;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var userVote = await _context.PostVotes
                    .FirstOrDefaultAsync(pv => pv.PostId == post.PostId && pv.UserId == currentUserId);
                currentUserVote = userVote?.VoteType;

                isSavedByUser = await _context.SavedPosts
                    .AnyAsync(sp => sp.PostId == post.PostId && sp.UserId == currentUserId);
            }

            var pollViewModel = new PollViewModel();
            if (post.HasPoll && post.PollOptions.Any())
            {
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
                    Question = post.Title,
                    Options = post.PollOptions.OrderBy(po => po.DisplayOrder).Select(po => new PollOptionViewModel
                    {
                        PollOptionId = po.PollOptionId,
                        OptionText = po.OptionText,
                        VoteCount = po.VoteCount,
                        VotePercentage = (decimal)(post.PollVoteCount > 0 ? (double)po.VoteCount / post.PollVoteCount * 100 : 0),
                        IsSelected = userPollVotes.Contains(po.PollOptionId)
                    }).ToList(),
                    TotalVotes = post.PollVoteCount,
                    AllowMultipleChoices = post.PollConfiguration != null ? post.PollConfiguration.AllowMultipleChoices : false,
                    ShowResultsBeforeVoting = post.PollConfiguration != null ? post.PollConfiguration.ShowResultsBeforeVoting : true,
                    ShowResultsBeforeEnd = post.PollConfiguration != null ? post.PollConfiguration.ShowResultsBeforeEnd : true,
                    AllowAddingOptions = post.PollConfiguration != null ? post.PollConfiguration.AllowAddingOptions : false,
                    EndDate = post.PollExpiresAt ?? post.PollConfiguration?.EndDate,
                    IsExpired = (post.PollExpiresAt.HasValue && post.PollExpiresAt < DateTime.UtcNow) ||
                               (post.PollConfiguration?.EndDate.HasValue == true && post.PollConfiguration.EndDate < DateTime.UtcNow),
                    HasUserVoted = userPollVotes.Any(),
                    UserVotedOptionIds = userPollVotes,
                    UserVotes = userPollVotes
                };
            }

            var linkModel = new LinkPreviewViewModel();
            if (post.PostType == "link" && !string.IsNullOrEmpty(post.Url))
            {
                // Logic for link preview
            }

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
                IsSavedByUser = isSavedByUser,
                UserId = post.UserId,
                AuthorDisplayName = post.UserProfile?.DisplayName ?? "Unknown",
                AuthorInitials = GetInitials(post.UserProfile?.DisplayName ?? "Unknown"),
                AuthorKarma = post.UserProfile?.KarmaPoints ?? 0,
                IsCurrentUserAuthor = post.UserId == currentUserId,
                CommunityId = post.CommunityId,
                CommunityName = post.Community!.Name,
                CommunitySlug = post.Community.Slug,
                CommunityIconUrl = post.Community.IconUrl,
                CategorySlug = post.Community.Category?.Slug,
                Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                Media = post.Media.Select(m => new MediaViewModel
                {
                    MediaId = m.MediaId,
                    Url = m.Url,
                    ThumbnailUrl = m.ThumbnailUrl,
                    MediaType = m.MediaType,
                    Caption = m.Caption,
                    AltText = m.AltText,
                    Width = (int)m.Width,
                    Height = m.Height ?? 0
                }).ToList(),
                Poll = pollViewModel,
                LinkModel = linkModel,
                Awards = post.Awards.Select(pa => new PostAwardViewModel
                {
                    PostAwardId = pa.PostAwardId,
                    AwardName = pa.Award.Name,
                    AwardIconUrl = pa.Award.IconUrl,
                    AwardedAt = pa.AwardedAt,
                    IsAnonymous = pa.IsAnonymous,
                    Message = pa.Message
                }).ToList(),
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

        public async Task<CreatePostResult> CreatePostAsync(CreatePostViewModel model)
        {
            var slug = model.Title.ToSlug();
            string currentUserId = string.Empty;
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
                HasPoll = model.PostType == "poll",
                PollOptionCount = model.PostType == "poll" ? (model.PollOptions?.Count ?? 0) : 0,
                PollVoteCount = 0
            };

            if (model.PostType == "poll" && model.PollExpiresAt.HasValue)
            {
                post.PollExpiresAt = model.PollExpiresAt.Value;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var pollViewModel = new PollViewModel();
            if (post.HasPoll && post.PollOptions.Any())
            {
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
                    Question = post.Title,
                    Options = post.PollOptions.OrderBy(po => po.DisplayOrder).Select(po => new PollOptionViewModel
                    {
                        PollOptionId = po.PollOptionId,
                        OptionText = po.OptionText,
                        VoteCount = po.VoteCount,
                        VotePercentage = (decimal)(post.PollVoteCount > 0 ? (double)po.VoteCount / post.PollVoteCount * 100 : 0),
                        IsSelected = userPollVotes.Contains(po.PollOptionId)
                    }).ToList(),
                    TotalVotes = post.PollVoteCount,
                    AllowMultipleChoices = post.PollConfiguration?.AllowMultipleChoices ?? false,
                    ShowResultsBeforeVoting = post.PollConfiguration?.ShowResultsBeforeVoting ?? true,
                    ShowResultsBeforeEnd = post.PollConfiguration?.ShowResultsBeforeEnd ?? true,
                    AllowAddingOptions = post.PollConfiguration?.AllowAddingOptions ?? false,
                    MinOptions = post.PollConfiguration?.MinOptions ?? 2,
                    MaxOptions = post.PollConfiguration?.MaxOptions ?? 10,
                    EndDate = post.PollExpiresAt ?? post.PollConfiguration?.EndDate,
                    IsExpired = (post.PollExpiresAt.HasValue && post.PollExpiresAt < DateTime.UtcNow) ||
                               (post.PollConfiguration?.EndDate.HasValue == true && post.PollConfiguration.EndDate < DateTime.UtcNow),
                    HasUserVoted = userPollVotes.Any(),
                    UserVotedOptionIds = userPollVotes,
                    UserVotes = userPollVotes
                };
            }

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
                            MediaType = DetermineMediaType(mediaUrl),
                            UploadedAt = DateTime.UtcNow,
                            StorageProvider = "external",
                            IsProcessed = true
                        };
                        _context.Media.Add(media);
                    }
                }
            }

            if (!string.IsNullOrEmpty(model.TagsInput))
            {
                var tagNames = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToList();

                foreach (var tagName in tagNames)
                {
                    var existingTag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName);

                    Tag tag;
                    if (existingTag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            Slug = tagName.ToSlug(),
                            CreatedAt = DateTime.UtcNow,
                            PostCount = 1
                        };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        tag = existingTag;
                        tag.PostCount++;
                    }

                    var postTag = new PostTag
                    {
                        PostId = post.PostId,
                        TagId = tag.TagId
                    };
                    _context.PostTags.Add(postTag);
                }
            }

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
                PostSlug = slug

            };
        }

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
                    _context.PostVotes.Remove(existingVote);
                    if (voteType == 1) post.UpvoteCount--;
                    else post.DownvoteCount--;
                    voteType = 0;
                }
                else
                {
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
                Tags = p.PostTags?.Select(pt => pt.Tag?.Name ?? string.Empty).ToList() ?? new List<string>(),
                IsSavedByUser = false
            };

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
                var existingVotes = await _context.PollVotes
                    .Where(pv => pv.PollOption.PostId == postId && pv.UserId == userId)
                    .ToListAsync();

                _context.PollVotes.RemoveRange(existingVotes);

                var newVotes = optionIds.Select(optionId => new PollVote
                {
                    PollOptionId = optionId,
                    UserId = userId,
                    VotedAt = DateTime.UtcNow
                }).ToList();

                _context.PollVotes.AddRange(newVotes);
                await _context.SaveChangesAsync();

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
            return await GetPollDetailsAsync(postId, null);
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
    }
}
