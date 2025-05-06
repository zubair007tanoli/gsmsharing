using discussionspot.Extensions;
using discussionspot.Models.Domain;
using discussionspot.Repositories;
using discussionspot.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Services
{
    /// <summary>
    /// Service for post-related operations, integrating both EF and ADO.NET repositories
    /// </summary>
    public class PostService
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

        /// <summary>
        /// Gets a post by ID with relationships (uses EF by default but can use ADO for optimization)
        /// </summary>
        public async Task<Post?> GetPostByIdAsync(int postId, bool useAdoOptimization = false)
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

                if (query != null)
                {
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

            return null;
        }

        /// <summary>
        /// Gets a post by slug and community slug with relationships
        /// </summary>
        public async Task<Post?> GetPostBySlugAsync(string postSlug, string communitySlug, bool useAdoOptimization = false)
        {
            if (useAdoOptimization)
            {
                // Get the community ID first
                var communityId = await _efRepository.GetDbContext().Communities
                    .Where(c => c.Slug == communitySlug)
                    .Select(c => c.CommunityId)
                    .FirstOrDefaultAsync();

                if (communityId == 0) return null;

                // Get post ID
                var postId = await _efRepository.GetDbContext().Posts
                    .Where(p => p.Slug == postSlug && p.CommunityId == communityId)
                    .Select(p => p.PostId)
                    .FirstOrDefaultAsync();

                if (postId == 0) return null;

                // Use ADO.NET optimization to get full post details
                return await _adoRepository.GetPostWithDetailsAsync(postId);
            }
            else
            {
                // Using Entity Framework with eager loading
                return await _efRepository.AsQueryable()
                    .Include(p => p.User)
                    .Include(p => p.Community)
                    .Include(p => p.Media)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Include(p => p.PollOptions)
                    .Include(p => p.PollConfiguration)
                    .Include(p => p.SeoMetadata)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(p => p.Slug == postSlug && p.Community.Slug == communitySlug);
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

        /// <summary>
        /// Updates a post's view count and returns the post
        /// Uses ADO.NET for direct database update for better performance
        /// </summary>
        public async Task<Post?> ViewPostAsync(int postId)
        {
            // Update view count with ADO.NET for better performance
            using (var connection = await _adoRepository.CreateConnectionAsync())
            using (var command = new Microsoft.Data.SqlClient.SqlCommand(
                "UPDATE Post SET ViewCount = ViewCount + 1 WHERE PostId = @PostId", connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);
                await command.ExecuteNonQueryAsync();
            }

            // Get updated post with details
            return await GetPostByIdAsync(postId, true); // Use ADO.NET optimization
        }

        /// <summary>
        /// Vote on a post
        /// </summary>
        public async Task<bool> VotePostAsync(int postId, string userId, int voteType)
        {
            var post = await _efRepository.GetByIdAsync(postId);
            if (post == null)
                return false;

            var dbContext = _efRepository.GetDbContext();

            // Check if user already voted
            var existingVote = await dbContext.PostVotes
                .FirstOrDefaultAsync(pv => pv.PostId == postId && pv.UserId == userId);

            if (existingVote != null)
            {
                // If same vote type, remove the vote (toggle)
                if (existingVote.VoteType == voteType)
                {
                    dbContext.PostVotes.Remove(existingVote);

                    // Update vote counts
                    if (voteType == 1)
                        post.UpvoteCount--;
                    else
                        post.DownvoteCount--;
                }
                else
                {
                    // Change vote type
                    existingVote.VoteType = voteType;
                    existingVote.VotedAt = DateTime.UtcNow;

                    // Update vote counts
                    if (voteType == 1)
                    {
                        post.UpvoteCount++;
                        post.DownvoteCount--;
                    }
                    else
                    {
                        post.UpvoteCount--;
                        post.DownvoteCount++;
                    }
                }
            }
            else
            {
                // Create new vote
                var vote = _modelFactory.CreatePostVote(postId, userId, voteType);
                await dbContext.PostVotes.AddAsync(vote);

                // Update vote counts
                if (voteType == 1)
                    post.UpvoteCount++;
                else
                    post.DownvoteCount++;
            }

            // Update score
            post.Score = post.UpvoteCount - post.DownvoteCount;
            post.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Vote on a poll option
        /// </summary>
        public async Task<bool> VotePollAsync(int pollOptionId, string userId)
        {
            var dbContext = _efRepository.GetDbContext();

            // Get the poll option and post
            var pollOption = await dbContext.PollOptions
                .Include(po => po.Post)
                .ThenInclude(p => p.PollConfiguration)
                .FirstOrDefaultAsync(po => po.PollOptionId == pollOptionId);

            if (pollOption == null || pollOption.Post == null)
                return false;

            // Check if poll has ended
            if (pollOption.Post.PollConfiguration?.HasEnded == true)
                return false;

            // Check if user already voted
            var existingVotes = await dbContext.PollVotes
                .Where(pv => pv.PollOption.PostId == pollOption.PostId && pv.UserId == userId)
                .ToListAsync();

            // If user already voted and multiple choices not allowed, return false
            if (existingVotes.Any() &&
                (pollOption.Post.PollConfiguration?.AllowMultipleChoices == false ||
                 existingVotes.Any(v => v.PollOptionId == pollOptionId)))
            {
                // User already voted on this option or multiple votes not allowed
                return false;
            }

            // Create new vote
            var vote = _modelFactory.CreatePollVote(pollOptionId, userId);
            await dbContext.PollVotes.AddAsync(vote);

            // Update vote counts
            pollOption.VoteCount++;
            pollOption.Post.PollVoteCount++;

            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}
