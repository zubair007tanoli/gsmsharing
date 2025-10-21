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
            _logger.LogInformation($"🎯 [START] CastPollVoteAsync - PostId: {postId}, OptionId: {pollOptionId}, UserId: {userId}");
            
            if (pollOptionId <= 0)
            {
                _logger.LogError($"❌ Invalid poll option ID: {pollOptionId}");
                return new VotePollResult { Success = false, Message = "Invalid poll option selected." };
            }

            // CRITICAL FIX: Use execution strategy to handle transactions with retry logic
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation($"🔍 Step 1: Checking if post {postId} exists...");
                    var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
                if (post == null)
                {
                    _logger.LogError($"❌ Post not found: {postId}");
                    return new VotePollResult { Success = false, Message = "Post not found." };
                }
                _logger.LogInformation($"✅ Post found: {post.Title}, HasPoll: {post.HasPoll}");

                if (!post.HasPoll)
                {
                    _logger.LogError($"❌ Post {postId} is not a poll.");
                    return new VotePollResult { Success = false, Message = "This post is not a poll." };
                }

                _logger.LogInformation($"🔍 Step 2: Loading poll configuration...");
                var pollConfig = await _context.PollConfigurations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pc => pc.PostId == postId);

                if (pollConfig == null)
                {
                    _logger.LogWarning($"⚠️ Poll configuration not found for PostId: {postId}. Creating default configuration.");
                    pollConfig = new PollConfiguration
                    {
                        PostId = postId,
                        AllowMultipleChoices = false,
                        ShowResultsBeforeVoting = true,
                        ShowResultsBeforeEnd = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.PollConfigurations.Add(pollConfig);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Created default poll configuration");
                }
                else
                {
                    _logger.LogInformation($"✅ Poll config found - AllowMultiple: {pollConfig.AllowMultipleChoices}");
                }

                _logger.LogInformation($"🔍 Step 3: Validating poll option exists...");
                var pollOptionExists = await _context.PollOptions
                    .AnyAsync(po => po.PollOptionId == pollOptionId && po.PostId == postId);
                
                if (!pollOptionExists)
                {
                    _logger.LogError($"❌ Poll option {pollOptionId} not found for post {postId}");
                    
                    // Log all available options for debugging
                    var availableOptions = await _context.PollOptions
                        .Where(po => po.PostId == postId)
                        .Select(po => po.PollOptionId)
                    .ToListAsync();
                    _logger.LogError($"📋 Available poll options for post {postId}: {string.Join(", ", availableOptions)}");
                    
                    return new VotePollResult { Success = false, Message = "Invalid poll option." };
                }
                _logger.LogInformation($"✅ Poll option {pollOptionId} validated");

                _logger.LogInformation($"🔍 Step 4: Loading user's existing votes...");
                // FIXED: Use explicit JOIN instead of navigation property to avoid Include() issues
                var userVotesForPost = await (from pv in _context.PollVotes
                                              join po in _context.PollOptions on pv.PollOptionId equals po.PollOptionId
                                              where pv.UserId == userId && po.PostId == postId
                                              select pv).ToListAsync();
                _logger.LogInformation($"📊 User has {userVotesForPost.Count} existing vote(s) for this poll");

                var existingVoteForOption = userVotesForPost.FirstOrDefault(v => v.PollOptionId == pollOptionId);

                if (existingVoteForOption != null)
                {
                    _logger.LogInformation($"🔄 Step 5: User is REMOVING their vote for option {pollOptionId}");
                    _context.PollVotes.Remove(existingVoteForOption);
                }
                else
                {
                    _logger.LogInformation($"➕ Step 5: User is ADDING a new vote for option {pollOptionId}");
                    if (!pollConfig.AllowMultipleChoices && userVotesForPost.Any())
                    {
                        _logger.LogInformation($"🗑️ Removing {userVotesForPost.Count} existing vote(s) (single-choice poll)");
                        _context.PollVotes.RemoveRange(userVotesForPost);
                    }

                    var newVote = new PollVote
                    {
                        PollOptionId = pollOptionId,
                        UserId = userId,
                        VotedAt = DateTime.UtcNow
                    };
                    _context.PollVotes.Add(newVote);
                    _logger.LogInformation($"✅ New vote object created");
                }

                _logger.LogInformation($"💾 Step 6: Saving vote changes to database...");
                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Vote changes saved");

                _logger.LogInformation($"🔍 Step 7: Recalculating vote counts for all options...");
                var pollOptions = await _context.PollOptions.Where(po => po.PostId == postId).ToListAsync();
                _logger.LogInformation($"📊 Found {pollOptions.Count} poll options to update");
                
                foreach (var option in pollOptions)
                {
                    var oldCount = option.VoteCount;
                    option.VoteCount = await _context.PollVotes.CountAsync(v => v.PollOptionId == option.PollOptionId);
                    _logger.LogInformation($"   Option {option.PollOptionId} '{option.OptionText}': {oldCount} → {option.VoteCount} votes");
                }

                var totalVotes = pollOptions.Sum(po => po.VoteCount);
                post.PollVoteCount = totalVotes;
                _logger.LogInformation($"📊 Total poll votes calculated: {totalVotes}");
                
                // Log percentage calculation for debugging
                foreach (var option in pollOptions)
                {
                    var percentage = totalVotes > 0 ? (decimal)option.VoteCount / totalVotes * 100 : 0;
                    _logger.LogInformation($"   Option {option.PollOptionId} percentage: {option.VoteCount}/{totalVotes} = {percentage:F1}%");
                }

                _logger.LogInformation($"💾 Step 8: Saving updated vote counts...");
                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Vote counts saved");
                
                _logger.LogInformation($"✅ Step 9: Committing transaction...");
                await transaction.CommitAsync();
                _logger.LogInformation($"✅ Transaction committed successfully");

                var updatedVoteCounts = pollOptions
                    .ToDictionary(x => x.PollOptionId, x => x.VoteCount);

                _logger.LogInformation($"🎉 [SUCCESS] Poll vote completed - PostId: {postId}, OptionId: {pollOptionId}");
                    return new VotePollResult
                    {
                        Success = true,
                        UpdatedVoteCounts = updatedVoteCounts
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"❌❌❌ [EXCEPTION] Error casting poll vote - PostId: {postId}, OptionId: {pollOptionId}, UserId: {userId}");
                    _logger.LogError($"❌ Exception Type: {ex.GetType().Name}");
                    _logger.LogError($"❌ Exception Message: {ex.Message}");
                    _logger.LogError($"❌ Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        _logger.LogError($"❌ Inner Exception: {ex.InnerException.Message}");
                    }
                    return new VotePollResult
                    {
                        Success = false,
                        Message = "An error occurred while processing your vote."
                    };
                }
            });
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
                PollExpiresAt = model.PollEndDate ?? model.PollEndDate,
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
                EndDate = model.PollEndDate,
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

            await _context.SaveChangesAsync();
        }

        public async Task<PostDetailViewModel?> GetPostDetailsUpdateAsync(string communitySlug, string postSlug, string? currentUserId = null)
        {
            var post = await _context.Posts
                .Include(p => p.Community)
                    .ThenInclude(c => c.Category)
                .Include(p => p.UserProfile)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Media)
                .Include(p => p.PollOptions)
                    .ThenInclude(po => po.Votes)
                .Include(p => p.PollConfiguration)
                .Include(p => p.Awards)
                    .ThenInclude(pa => pa.Award)
                .Include(p => p.Votes)
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

            var pollViewModel = await GetPollDetailsAsync(post.PostId, currentUserId);

            var linkModel = new LinkPreviewViewModel();
            if (post.PostType == "link" && !string.IsNullOrEmpty(post.Url))
            {
                try
                {
                    var uri = new Uri(post.Url);
                    linkModel.Title = post.Title;
                    linkModel.Description = $"A link to {uri.Host}.";
                    linkModel.Url = uri.Host;
                }
                catch (UriFormatException)
                {
                    linkModel.Title = post.Title;
                    linkModel.Description = "An external link.";
                    linkModel.Url = "invalid.url";
                }
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
                //Score = post.Score,
                HasPoll = post.HasPoll,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                IsNSFW = post.IsNSFW,
                IsSpoiler = post.IsSpoiler,
                IsSavedByUser = isSavedByUser,
                UserId = post.UserId,
                AuthorDisplayName = post.UserProfile?.DisplayName ?? "Unknown",
                //AuthorUrl = $"/user/{post.UserProfile?.DisplayName}",
                AuthorInitials = GetInitials(post.UserProfile?.DisplayName ?? "Unknown"),
                AuthorKarma = post.UserProfile?.KarmaPoints ?? 0,
                IsCurrentUserAuthor = post.UserId == currentUserId,
                CommunityId = post.CommunityId,
                CommunityName = post.Community!.Name,
                CommunitySlug = post.Community.Slug,
                CommunityIconUrl = post.Community.IconUrl,
                CategorySlug = post.Community.Category?.Slug,
                Tags = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                // Poll data to populate PollViewModel
                Media = post.Media.Select(m => new MediaViewModel
                {
                    MediaId = m.MediaId,
                    Url = m.Url,
                    ThumbnailUrl = m.ThumbnailUrl,
                    MediaType = m.MediaType,
                    Caption = m.Caption,
                    AltText = m.AltText,
                    Width = m.Width ?? 0,      // ← Use null-coalescing operator
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

            if (model.PostType == "poll" && model.PollEndDate.HasValue)
            {
                post.PollExpiresAt = model.PollEndDate.Value;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var pollViewModel = new PollViewModel();
            if (post.HasPoll && post.PollOptions.Any())
            {
                List<int> userPollVotes = new();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    // FIXED: Use explicit JOIN instead of navigation property
                    userPollVotes = await (from pv in _context.PollVotes
                                          join po in _context.PollOptions on pv.PollOptionId equals po.PollOptionId
                                          where pv.UserId == currentUserId && po.PostId == post.PostId
                                          select pv.PollOptionId).ToListAsync();
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
            try
            {
                _logger.LogInformation($"🗳️ VotePostAsync called - PostId: {postId}, UserId: {userId}, VoteType: {voteType}");
                
                var existingVote = await _context.PostVotes
                    .FirstOrDefaultAsync(pv => pv.PostId == postId && pv.UserId == userId);

                _logger.LogInformation($"📊 Existing vote: {(existingVote != null ? $"Found (Type: {existingVote.VoteType})" : "Not found")}");

                var post = await _context.Posts.FindAsync(postId);
                if (post == null)
                {
                    _logger.LogError($"❌ Post {postId} not found");
                    return new VoteResult { Success = false, ErrorMessage = "Post not found" };
                }

                _logger.LogInformation($"📊 Post before vote - Up: {post.UpvoteCount}, Down: {post.DownvoteCount}, Score: {post.Score}");

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    // Removing vote
                    _logger.LogInformation($"➖ Removing existing {(voteType == 1 ? "upvote" : "downvote")}");
                    _context.PostVotes.Remove(existingVote);
                    if (voteType == 1) post.UpvoteCount = Math.Max(0, post.UpvoteCount - 1);
                    else post.DownvoteCount = Math.Max(0, post.DownvoteCount - 1);
                    voteType = 0;
                }
                else
                {
                    // Changing vote
                    _logger.LogInformation($"🔄 Changing vote from {existingVote.VoteType} to {voteType}");
                    if (existingVote.VoteType == 1) post.UpvoteCount = Math.Max(0, post.UpvoteCount - 1);
                    else post.DownvoteCount = Math.Max(0, post.DownvoteCount - 1);

                    existingVote.VoteType = voteType;
                    existingVote.VotedAt = DateTime.UtcNow;

                    if (voteType == 1) post.UpvoteCount++;
                    else post.DownvoteCount++;
                }
            }
            else
            {
                // New vote
                _logger.LogInformation($"➕ Adding new {(voteType == 1 ? "upvote" : "downvote")}");
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
            post.UpdatedAt = DateTime.UtcNow;
            
            _logger.LogInformation($"📊 Post after vote - Up: {post.UpvoteCount}, Down: {post.DownvoteCount}, Score: {post.Score}");
            
            // Explicitly mark specific properties as modified
            var postEntry = _context.Entry(post);
            postEntry.Property(p => p.UpvoteCount).IsModified = true;
            postEntry.Property(p => p.DownvoteCount).IsModified = true;
            postEntry.Property(p => p.Score).IsModified = true;
            postEntry.Property(p => p.UpdatedAt).IsModified = true;
            
            _logger.LogInformation($"🔄 Entity state: {postEntry.State}");
            _logger.LogInformation($"💾 Saving changes to database...");
            
            var savedCount = await _context.SaveChangesAsync();
            
            _logger.LogInformation($"✅ SaveChanges completed. Entities saved: {savedCount}");
            
            if (savedCount == 0)
            {
                _logger.LogWarning($"⚠️ WARNING: SaveChanges returned 0 - no entities were saved!");
            }
            
            // Detach the entity to ensure GetPostByIdAsync gets fresh data
            _context.Entry(post).State = EntityState.Detached;

                return new VoteResult { Success = true, UserVote = voteType == 0 ? null : voteType };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error in VotePostAsync for post {postId}");
                return new VoteResult { Success = false, ErrorMessage = $"Failed to save vote: {ex.Message}" };
            }
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
                // FIXED: Use explicit JOIN instead of navigation property
                var existingVotes = await (from pv in _context.PollVotes
                                          join po in _context.PollOptions on pv.PollOptionId equals po.PollOptionId
                                          where pv.UserId == userId && po.PostId == postId
                                          select pv).ToListAsync();

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
            // FIXED: Use explicit JOIN instead of navigation property
            return await (from pv in _context.PollVotes
                         join po in _context.PollOptions on pv.PollOptionId equals po.PollOptionId
                         where pv.UserId == userId && po.PostId == postId
                         select pv).AnyAsync();
        }

        public async Task<List<int>> GetUserPollVotesAsync(int postId, string userId)
        {
            // FIXED: Use explicit JOIN instead of navigation property
            return await (from pv in _context.PollVotes
                         join po in _context.PollOptions on pv.PollOptionId equals po.PollOptionId
                         where pv.UserId == userId && po.PostId == postId
                         select pv.PollOptionId).ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            // Use AsNoTracking to ensure we get fresh data from database
            // FindAsync can return cached/tracked entities with stale vote counts
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PostId == postId);
            
            return post ?? throw new KeyNotFoundException($"Post with ID {postId} not found.");
        }

        public async Task<PollViewModel?> GetPollDetailsAsync(int postId, string? userId)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Include(p => p.PollOptions)
                    .ThenInclude(po => po.Votes)
                .Include(p => p.PollConfiguration)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null || !post.HasPoll || post.PollOptions == null || !post.PollOptions.Any())
                return null;

            // CRITICAL FIX: Use VoteCount property instead of Votes.Count
            // The VoteCount is updated in CastPollVoteAsync and is the source of truth
            var totalVotes = post.PollOptions.Sum(po => po.VoteCount);
            _logger.LogInformation($"📊 GetPollDetailsAsync - Total votes: {totalVotes}");

            List<int> userVotedOptionIds = new List<int>();
            if (userId != null)
            {
                // FIXED: Use explicit JOIN instead of navigation property
                userVotedOptionIds = await (from pv in _context.PollVotes
                                           join po in _context.PollOptions on pv.PollOptionId equals po.PollOptionId
                                           where pv.UserId == userId && po.PostId == postId
                                           select pv.PollOptionId).ToListAsync();
            }
            var hasUserVoted = userVotedOptionIds.Any();

            // Map all poll configuration fields
            return new PollViewModel
            {
                PostId = post.PostId,
                Question = post.PollConfiguration?.PollQuestion ?? post.Title,
                PollDescription = post.PollConfiguration?.PollDescription,
                ClosedByUserId = post.PollConfiguration?.ClosedByUserId,
                ClosedAt = post.PollConfiguration?.ClosedAt,
                TotalVotes = totalVotes,
                HasUserVoted = hasUserVoted,
                UserVotes = userVotedOptionIds,
                EndDate = post.PollConfiguration?.EndDate,
                AllowMultipleChoices = post.PollConfiguration?.AllowMultipleChoices ?? false,
                ShowResultsBeforeVoting = post.PollConfiguration?.ShowResultsBeforeVoting ?? true,
                ShowResultsBeforeEnd = post.PollConfiguration?.ShowResultsBeforeEnd ?? true,
                AllowAddingOptions = post.PollConfiguration?.AllowAddingOptions ?? false,
                MinOptions = post.PollConfiguration?.MinOptions ?? 2,
                MaxOptions = post.PollConfiguration?.MaxOptions ?? 10,
                IsExpired = (post.PollConfiguration?.EndDate.HasValue == true && post.PollConfiguration.EndDate < DateTime.UtcNow),
                Options = post.PollOptions.Select(po => new PollOptionViewModel
                {
                    PollOptionId = po.PollOptionId,
                    OptionText = po.OptionText,
                    VoteCount = po.VoteCount, // FIXED: Use VoteCount property, not Votes.Count
                    HasUserVoted = userVotedOptionIds.Contains(po.PollOptionId),
                    VotePercentage = totalVotes > 0 ? (decimal)po.VoteCount / totalVotes * 100 : 0, // FIXED: Use VoteCount
                    DisplayOrder = po.DisplayOrder
                })
                .OrderBy(o => o.DisplayOrder)
                .ToList()
            };
        }
    }
}

