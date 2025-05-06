using discussionspot.Extensions;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.Repositories;
using discussionspot.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace discussionspot.Services
{
    /// <summary>
    /// Service for post-related operations, integrating both EF and ADO.NET repositories
    /// </summary>
    public class PostService : IPostService
    {
        private readonly EfRepository<Post> _efRepository;
        private readonly PostAdoRepository _adoRepository;
        private readonly ModelFactory _modelFactory;

        public PostService(
            EfRepository<Post> efRepository,
            PostAdoRepository adoRepository,
            ModelFactory modelFactory)
        {
            _efRepository = efRepository;
            _adoRepository = adoRepository;
            _modelFactory = modelFactory;
        }

        #region IPostService Implementation

        /// <summary>
        /// Gets posts based on sort criteria, time filter, and optional community
        /// </summary>
        public async Task<IEnumerable<PostViewModel>> GetPostsAsync(string sortBy, string timeFilter, int? communityId)
        {
            List<Post> posts;

            // Use the appropriate method based on sort type
            if (sortBy.ToLower() == "hot" || sortBy.ToLower() == "trending")
            {
                posts = await GetTrendingPostsAsync(communityId, GetDaysFromTimeFilter(timeFilter));
            }
            else if (sortBy.ToLower() == "new" || sortBy.ToLower() == "latest")
            {
                posts = await GetLatestPostsAsync(communityId);
            }
            else
            {
                // Default to trending
                posts = await GetTrendingPostsAsync(communityId);
            }

            // Map to view models
            return posts.Select(p => MapPostToViewModel(p));
        }

        /// <summary>
        /// Gets a specific post by ID and converts to view model
        /// </summary>
        public async Task<PostViewModel> GetPostByIdAsync(int id)
        {
            var post = await GetPostEntityByIdAsync(id, true);
            return post != null ? MapPostToViewModel(post) : null;
        }

        /// <summary>
        /// Gets a post for editing
        /// </summary>
        public async Task<PostCreateViewModel> GetPostForEditingAsync(int postId)
        {
            var post = await GetPostEntityByIdAsync(postId);
            if (post == null)
                return null;

            return CreateEditViewModel(MapPostToViewModel(post));
        }

        /// <summary>
        /// Gets related posts based on tags, category, etc.
        /// </summary>
        public async Task<IEnumerable<PostViewModel>> GetRelatedPostsAsync(int postId, int count)
        {
            var post = await GetPostEntityByIdAsync(postId);
            if (post == null)
                return new List<PostViewModel>();

            // Get tags for the post
            var tags = post.PostTags?.Select(pt => pt.TagId).ToList() ?? new List<int>();

            // Find posts with the same tags or in the same community
            var relatedPosts = await _efRepository.AsQueryable()
                .Where(p => p.PostId != postId &&
                           (p.CommunityId == post.CommunityId ||
                            p.PostTags.Any(pt => tags.Contains(pt.TagId))))
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Include(p => p.User)
                .Include(p => p.Community)
                .ToListAsync();

            return relatedPosts.Select(p => MapPostToViewModel(p));
        }

        /// <summary>
        /// Gets posts by user
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsByUserIdAsync(string userId)
        {
            var posts = await _efRepository.AsQueryable()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Community)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Gets posts by community
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsByCommunityIdAsync(int communityId)
        {
            var posts = await _efRepository.AsQueryable()
                .Where(p => p.CommunityId == communityId)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Gets posts by category
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsByCategoryIdAsync(int categoryId)
        {
            var communityIds = await _efRepository.GetDbContext().Communities
                .Where(c => c.CategoryId == categoryId)
                .Select(c => c.CommunityId)
                .ToListAsync();

            var posts = await _efRepository.AsQueryable()
                .Where(p => communityIds.Contains(p.CommunityId))
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Community)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Searches for posts by keyword
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsBySearchTermAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<PostViewModel>();

            searchTerm = searchTerm.ToLower();

            var posts = await _efRepository.AsQueryable()
                .Where(p => p.Title.ToLower().Contains(searchTerm) ||
                            (p.Content != null && p.Content.ToLower().Contains(searchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Community)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Gets posts by status (published, draft, etc.)
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsByStatusAsync(string status)
        {
            var posts = await _efRepository.AsQueryable()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Community)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Gets posts by type (text, link, image, poll)
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsByTypeAsync(string type)
        {
            var posts = await _efRepository.AsQueryable()
                .Where(p => p.PostType == type)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Community)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Gets paginated posts
        /// </summary>
        public async Task<List<PostViewModel>> GetPostsByPaginationAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            var posts = await _efRepository.GetPagedAsync(
                skip,
                pageSize,
                null,
                p => p.CreatedAt,
                false);

            return posts.Select(p => MapPostToViewModel(p)).ToList();
        }

        /// <summary>
        /// Gets comments for a post
        /// </summary>
        public async Task<IEnumerable<CommentViewModel>> GetCommentsForPostAsync(int postId)
        {
            var comments = await _efRepository.GetDbContext().Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => MapCommentToViewModel(c));
        }

        /// <summary>
        /// Creates a new post
        /// </summary>
        public async Task<int> CreatePostAsync(PostCreateViewModel model, string userId)
        {
            Post post;

            // Create different types of posts based on the post type
            switch (model.PostType.ToLower())
            {
                case "text":
                    post = await CreateTextPostAsync(model, userId);
                    break;
                case "link":
                    post = await CreateLinkPostAsync(model, userId);
                    break;
                case "image":
                    post = await CreateImagePostAsync(model, userId);
                    break;
                case "poll":
                    post = await CreatePollPostAsync(model, userId);
                    break;
                default:
                    post = await CreateTextPostAsync(model, userId);
                    break;
            }

            return post.PostId;
        }

        /// <summary>
        /// Updates an existing post
        /// </summary>
        public async Task<bool> UpdatePostAsync(int postId, PostCreateViewModel model, string userId)
        {
            var post = await GetPostEntityByIdAsync(postId);
            if (post == null)
                return false;

            // Security check - only the author or admin can update
            if (post.UserId != userId)
            {
                // Check if user is admin or moderator
                var isAdmin = await _efRepository.GetDbContext().CommunityMembers
                    .AnyAsync(cm => cm.CommunityId == post.CommunityId &&
                                     cm.UserId == userId &&
                                     (cm.Role == "admin" || cm.Role == "moderator"));

                if (!isAdmin)
                    return false;
            }

            // Update common properties
            post.Title = model.Title;
            post.Content = model.Content;
            post.IsNSFW = model.IsNSFW;
            post.IsSpoiler = model.IsSpoiler;
            post.IsPinned = model.IsPinned;
            post.UpdatedAt = DateTime.UtcNow;

            // Type-specific updates
            if (post.PostType == "link" && !string.IsNullOrEmpty(model.Url))
            {
                post.ExternalUrl = model.Url;
            }

            _efRepository.Update(post);

            // Update tags
            if (!string.IsNullOrEmpty(model.TagsString))
            {
                // Remove existing tags
                var existingTags = await _efRepository.GetDbContext().PostTags
                    .Where(pt => pt.PostId == postId)
                    .ToListAsync();

                _efRepository.GetDbContext().PostTags.RemoveRange(existingTags);
                await _efRepository.GetDbContext().SaveChangesAsync();

                // Add new tags
                await ProcessPostTagsAsync(postId, model.TagsString);
            }

            return true;
        }

        /// <summary>
        /// Deletes a post
        /// </summary>
        public async Task<bool> DeletePostAsync(int postId)
        {
            var post = await GetPostEntityByIdAsync(postId);
            if (post == null)
                return false;

            // Set status to deleted instead of actually deleting
            post.Status = "deleted";
            post.UpdatedAt = DateTime.UtcNow;

            _efRepository.Update(post);
            return true;
        }

        /// <summary>
        /// Gets user's vote for a post
        /// </summary>
        public async Task<bool?> GetUserVoteForPostAsync(int postId, string userId)
        {
            var vote = await _efRepository.GetDbContext().PostVotes
                .FirstOrDefaultAsync(pv => pv.PostId == postId && pv.UserId == userId);

            if (vote == null)
                return null;

            return vote.VoteType == 1;
        }

        /// <summary>
        /// Votes on a post
        /// </summary>
        public async Task<VoteResult> VotePostAsync(int postId, string userId, bool isUpvote)
        {
            // Convert boolean to vote type (1 for upvote, -1 for downvote)
            int voteType = isUpvote ? 1 : -1;

            // Use the existing implementation
            var success = await VotePostAsync(postId, userId, voteType);

            if (success)
            {
                // Get updated post
                var post = await GetPostEntityByIdAsync(postId);

                return new VoteResult
                {
                    UpvoteCount = post.UpvoteCount,
                    DownvoteCount = post.DownvoteCount,
                    UserVote = isUpvote
                };
            }

            return null;
        }

        /// <summary>
        /// Saves a post draft
        /// </summary>
        public async Task<int> SaveDraftAsync(PostCreateViewModel model, string userId)
        {
            // Create a post with draft status
            var post = _modelFactory.CreateTextPost(
                model.Title,
                model.Content,
                model.CommunityId,
                userId,
                model.IsNSFW,
                model.IsSpoiler);

            post.Status = "draft";

            await _efRepository.AddAsync(post);

            // Process tags if provided
            if (!string.IsNullOrEmpty(model.TagsString))
            {
                await ProcessPostTagsAsync(post.PostId, model.TagsString);
            }

            return post.PostId;
        }

        /// <summary>
        /// Gets user's post drafts
        /// </summary>
        public async Task<IEnumerable<PostDraftViewModel>> GetDraftsAsync(string userId)
        {
            var drafts = await _efRepository.AsQueryable()
                .Where(p => p.UserId == userId && p.Status == "draft")
                .OrderByDescending(p => p.UpdatedAt)
                .Include(p => p.Community)
                .ToListAsync();

            return drafts.Select(d => new PostDraftViewModel
            {
                PostId = d.PostId,
                Title = d.Title,
                Content = d.Content,
                CommunityId = d.CommunityId,
                CommunityName = d.Community?.Name,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                PostType = d.PostType
            });
        }

        /// <summary>
        /// Creates a view model for editing a post
        /// </summary>
        public PostCreateViewModel CreateEditViewModel(PostViewModel post)
        {
            var model = new PostCreateViewModel
            {
                Title = post.Title,
                Content = post.Content,
                CommunityId = post.CommunityId,
                PostType = post.PostType,
                Url = post.Url,
                IsNSFW = post.IsNSFW,
                IsSpoiler = post.IsSpoiler,
                IsPinned = post.IsPinned,
                TagsString = string.Join(",", post.Tags)
            };

            // Initialize post types
            model.InitializePostTypes();

            return model;
        }

        #endregion

        #region Existing Implementation

        /// <summary>
        /// Gets a post entity by ID (renamed from original GetPostByIdAsync to avoid conflict)
        /// </summary>
        private async Task<Post?> GetPostEntityByIdAsync(int postId, bool useAdoOptimization = false)
        {
            if (useAdoOptimization)
            {
                return await _adoRepository.GetPostWithDetailsAsync(postId);
            }
            else
            {
                // Using Entity Framework with eager loading
                var query = _efRepository.AsQueryable()
                    .Include(p => p.User)
                    .Include(p => p.Community)
                    .Include(p => p.Media)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Include(p => p.SeoMetadata)
                    .AsSplitQuery(); // Split the query to avoid cartesian explosion

                // If it's a poll post, include poll data
                var post = await query.FirstOrDefaultAsync(p => p.PostId == postId);

                if (post != null && post.HasPoll)
                {
                    // Load poll options and configuration in separate queries
                    post.PollOptions = await _efRepository.GetDbContext().PollOptions
                        .Where(po => po.PostId == postId)
                        .OrderBy(po => po.DisplayOrder)
                        .ToListAsync();

                    post.PollConfiguration = await _efRepository.GetDbContext().PollConfigurations
                        .FirstOrDefaultAsync(pc => pc.PostId == postId);
                }

                return post;
            }
        }

        /// <summary>
        /// Gets trending posts with optional community filtering
        /// Always uses ADO.NET for optimization as this is a complex, high-performance query
        /// </summary>
        public async Task<List<Post>> GetTrendingPostsAsync(int? communityId = null, int days = 7, int pageSize = 20, int pageNumber = 1)
        {
            // Use ADO.NET for this complex query with better performance
            return await _adoRepository.GetPopularPostsAsync(communityId, days, pageSize, pageNumber);
        }

        /// <summary>
        /// Gets latest posts with optional community filtering
        /// Always uses ADO.NET for optimization
        /// </summary>
        public async Task<List<Post>> GetLatestPostsAsync(int? communityId = null, int pageSize = 20, int pageNumber = 1)
        {
            // Use ADO.NET for better performance
            return await _adoRepository.GetLatestPostsAsync(communityId, pageSize, pageNumber);
        }

        /// <summary>
        /// Creates a text post
        /// </summary>
        public async Task<Post> CreateTextPostAsync(PostCreateViewModel model, string? userId = null)
        {
            // Use model factory to create post
            var post = _modelFactory.CreateTextPost(
                model.Title,
                model.Content,
                model.CommunityId,
                userId,
                model.IsNSFW,
                model.IsSpoiler);

            // Use Entity Framework for any complex operations with relationships
            await _efRepository.AddAsync(post);

            // Process tags if provided
            if (!string.IsNullOrEmpty(model.TagsString))
            {
                await ProcessPostTagsAsync(post.PostId, model.TagsString);
            }

            // Create SEO metadata
            var seoMetadata = _modelFactory.CreatePostSeoMetadata(post.PostId);
            await _efRepository.GetDbContext().SeoMetadata.AddAsync(seoMetadata);
            await _efRepository.GetDbContext().SaveChangesAsync();

            return post;
        }

        /// <summary>
        /// Creates a link post
        /// </summary>
        public async Task<Post> CreateLinkPostAsync(PostCreateViewModel model, string? userId = null)
        {
            if (string.IsNullOrEmpty(model.Url))
            {
                throw new ArgumentException("URL is required for link posts");
            }

            // Use model factory to create post
            var post = _modelFactory.CreateLinkPost(
                model.Title,
                model.Url,
                model.CommunityId,
                model.Content,
                userId,
                model.IsNSFW,
                model.IsSpoiler);

            // Use Entity Framework for any complex operations with relationships
            await _efRepository.AddAsync(post);

            // Process tags if provided
            if (!string.IsNullOrEmpty(model.TagsString))
            {
                await ProcessPostTagsAsync(post.PostId, model.TagsString);
            }

            // Create SEO metadata
            var seoMetadata = _modelFactory.CreatePostSeoMetadata(post.PostId);
            await _efRepository.GetDbContext().SeoMetadata.AddAsync(seoMetadata);
            await _efRepository.GetDbContext().SaveChangesAsync();

            return post;
        }

        /// <summary>
        /// Creates an image post with media
        /// </summary>
        public async Task<Post> CreateImagePostAsync(PostCreateViewModel model, string? userId = null)
        {
            // Use model factory to create post
            var post = _modelFactory.CreateImagePost(
                model.Title,
                model.CommunityId,
                model.Content,
                userId,
                model.IsNSFW,
                model.IsSpoiler);

            // Use Entity Framework for any complex operations with relationships
            await _efRepository.AddAsync(post);

            // Process images if provided
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                foreach (var imageFile in model.ImageFiles)
                {
                    // Process and save image
                    // This implementation depends on your file storage strategy
                    // Example: Azure Blob Storage, AWS S3, or local file system

                    // For this example, we'll assume images are saved locally
                    // In a real app, use a dedicated file storage service
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine("wwwroot", "uploads", "images", fileName);
                    var fileUrl = $"/uploads/images/{fileName}";

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Create media entity
                    var media = _modelFactory.CreateMedia(
                        fileUrl,
                        "image",
                        null,
                        post.PostId,
                        userId,
                        imageFile.ContentType,
                        imageFile.FileName,
                        imageFile.Length);

                    await _efRepository.GetDbContext().Media.AddAsync(media);
                }

                await _efRepository.GetDbContext().SaveChangesAsync();
            }

            // Process tags if provided
            if (!string.IsNullOrEmpty(model.TagsString))
            {
                await ProcessPostTagsAsync(post.PostId, model.TagsString);
            }

            // Create SEO metadata
            var seoMetadata = _modelFactory.CreatePostSeoMetadata(post.PostId);
            await _efRepository.GetDbContext().SeoMetadata.AddAsync(seoMetadata);
            await _efRepository.GetDbContext().SaveChangesAsync();

            return post;
        }

        /// <summary>
        /// Creates a poll post
        /// </summary>
        public async Task<Post> CreatePollPostAsync(PostCreateViewModel model, string? userId = null)
        {
            if (model.PollOptions == null || !model.PollOptions.Any())
            {
                throw new ArgumentException("Poll options are required for poll posts");
            }

            // Use model factory to create post
            var post = _modelFactory.CreatePollPost(
                model.Title,
                model.CommunityId,
                model.PollOptions,
                model.Content,
                userId,
                model.IsNSFW,
                model.IsSpoiler,
                model.AllowMultipleChoices,
                model.PollEndsAt);

            // Use Entity Framework for any complex operations with relationships
            await _efRepository.AddAsync(post);

            // Create poll configuration
            var pollConfig = _modelFactory.CreatePollConfiguration(
                post.PostId,
                model.AllowMultipleChoices,
                model.PollEndsAt,
                true, // Default: Show results before voting
                true, // Default: Show results before end
                false, // Default: Don't allow adding options
                2, // Default: Minimum 2 options
                10); // Default: Maximum 10 options

            await _efRepository.GetDbContext().PollConfigurations.AddAsync(pollConfig);

            // Create poll options
            for (int i = 0; i < model.PollOptions.Count; i++)
            {
                var option = _modelFactory.CreatePollOption(
                    post.PostId,
                    model.PollOptions[i],
                    i);

                await _efRepository.GetDbContext().PollOptions.AddAsync(option);
            }

            // Process tags if provided
            if (!string.IsNullOrEmpty(model.TagsString))
            {
                await ProcessPostTagsAsync(post.PostId, model.TagsString);
            }

            // Create SEO metadata
            var seoMetadata = _modelFactory.CreatePostSeoMetadata(post.PostId);
            await _efRepository.GetDbContext().SeoMetadata.AddAsync(seoMetadata);
            await _efRepository.GetDbContext().SaveChangesAsync();

            return post;
        }

        /// <summary>
        /// Processes tags for a post
        /// </summary>
        private async Task ProcessPostTagsAsync(int postId, string tagsString)
        {
            // Split the tags string
            var tagNames = tagsString.Split(',', ';')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .Take(5) // Limit to 5 tags
                .ToList();

            foreach (var tagName in tagNames)
            {
                // Check if tag exists
                var dbContext = _efRepository.GetDbContext();
                var tag = await dbContext.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

                // Create tag if it doesn't exist
                if (tag == null)
                {
                    tag = _modelFactory.CreateTag(tagName);
                    await dbContext.Tags.AddAsync(tag);
                    await dbContext.SaveChangesAsync();
                }

                // Create post tag relationship
                var postTag = _modelFactory.CreatePostTag(postId, tag.TagId);
                await dbContext.PostTags.AddAsync(postTag);
            }

            await _efRepository.GetDbContext().SaveChangesAsync();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Maps a Post entity to a PostViewModel
        /// </summary>
        private PostViewModel MapPostToViewModel(Post post)
        {
            if (post == null)
                return null;

            var viewModel = new PostViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Slug = post.Slug,
                UserId = post.UserId,
                Username = post.User?.UserName,
                CommunityId = post.CommunityId,
                CommunityName = post.Community?.Name,
                PostType = post.PostType,
                Url = post.ExternalUrl,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                UpvoteCount = post.UpvoteCount,
                DownvoteCount = post.DownvoteCount,
                CommentCount = post.CommentCount,
                Score = post.Score,
                Status = post.Status,
                IsPinned = post.IsPinned,
                IsLocked = post.IsLocked,
                IsNSFW = post.IsNSFW,
                IsSpoiler = post.IsSpoiler,
                ViewCount = post.ViewCount,
                HasPoll = post.HasPoll,
                Tags = post.PostTags?.Select(pt => pt.Tag?.Name).Where(n => n != null).ToList() ?? new List<string>(),
                Media = post.Media?.Select(m => new MediaViewModel
                {
                    MediaId = m.MediaId,
                    Url = m.Url,
                    ThumbnailUrl = m.ThumbnailUrl,
                    MediaType = m.MediaType,
                    Caption = m.Caption
                }).ToList() ?? new List<MediaViewModel>()
            };

            // Map poll options if this is a poll post
            if (post.HasPoll && post.PollOptions != null)
            {
                viewModel.PollOptions = post.PollOptions.Select(po => new PollOptionViewModel
                {
                    PollOptionId = po.PollOptionId,
                    OptionText = po.OptionText,
                    VoteCount = po.VoteCount,
                    DisplayOrder = po.DisplayOrder
                }).ToList();
            }

            return viewModel;
        }

        /// <summary>
        /// Maps a Comment entity to a CommentViewModel
        /// </summary>
        private CommentViewModel MapCommentToViewModel(Comment comment)
        {
            if (comment == null)
                return null;

            return new CommentViewModel
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                UserId = comment.UserId,
                Username = comment.User?.UserName,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UpvoteCount = comment.UpvoteCount,
                DownvoteCount = comment.DownvoteCount,
                Score = comment.Score,
                IsEdited = comment.IsEdited,
                IsDeleted = comment.IsDeleted,
                TreeLevel = comment.TreeLevel,
                ChildComments = new List<CommentViewModel>() // Child comments would be populated separately
            };
        }

        /// <summary>
        /// Converts a time filter string to a number of days
        /// </summary>
        private int GetDaysFromTimeFilter(string timeFilter)
        {
            return timeFilter.ToLower() switch
            {
                "today" => 1,
                "week" => 7,
                "month" => 30,
                "year" => 365,
                _ => 7 // Default to one week
            };
        }

        #endregion
    }
}
