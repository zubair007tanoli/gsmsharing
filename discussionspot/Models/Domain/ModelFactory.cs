using discussionspot.Data;
using discussionspot.Data.discussionspot.Data;
using System.Security.Claims;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Factory class for creating models - helps separate data access and model creation
    /// Makes it easier to switch between ADO.NET and Entity Framework
    /// </summary>
    public class ModelFactory
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModelFactory(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the current logged-in user ID
        /// </summary>
        private string? CurrentUserId
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }

        #region User Methods

        /// <summary>
        /// Creates a new user profile
        /// </summary>
        public UserProfile CreateUserProfile(string userId, string displayName)
        {
            return new UserProfile
            {
                UserId = userId,
                DisplayName = displayName,
                JoinDate = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            };
        }

        #endregion

        #region Category Methods

        /// <summary>
        /// Creates a new category
        /// </summary>
        public Category CreateCategory(string name, string? description = null, int? parentCategoryId = null, int displayOrder = 0)
        {
            var category = new Category
            {
                Name = name,
                Description = description,
                ParentCategoryId = parentCategoryId,
                DisplayOrder = displayOrder,
                IsActive = true
            };

            category.GenerateSlug();
            return category;
        }

        #endregion

        #region Community Methods

        /// <summary>
        /// Creates a new community
        /// </summary>
        public Community CreateCommunity(string name, string title, string? description = null, string? shortDescription = null,
            int? categoryId = null, string? creatorId = null, string communityType = "public", string? iconUrl = null,
            string? bannerUrl = null, string? themeColor = null, string? rules = null, bool isNSFW = false)
        {
            var community = new Community
            {
                Name = name,
                Title = title,
                Description = description,
                ShortDescription = shortDescription,
                CategoryId = categoryId,
                CreatorId = creatorId ?? CurrentUserId,
                CommunityType = communityType,
                IconUrl = iconUrl,
                BannerUrl = bannerUrl,
                ThemeColor = themeColor,
                Rules = rules,
                IsNSFW = isNSFW,
                MemberCount = 0,
                PostCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            community.GenerateSlug();
            return community;
        }

        /// <summary>
        /// Creates a community membership
        /// </summary>
        public CommunityMember CreateCommunityMember(int communityId, string? userId = null, string role = "member",
            string notificationPreference = "all")
        {
            return new CommunityMember
            {
                CommunityId = communityId,
                UserId = userId ?? CurrentUserId ?? throw new InvalidOperationException("User ID is required"),
                Role = role,
                NotificationPreference = notificationPreference,
                JoinedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Post Methods

        /// <summary>
        /// Creates a new text post
        /// </summary>
        public Post CreateTextPost(string title, string? content, int communityId, string? userId = null,
            bool isNSFW = false, bool isSpoiler = false)
        {
            var post = new Post
            {
                Title = title,
                Content = content,
                CommunityId = communityId,
                UserId = userId ?? CurrentUserId,
                PostType = "text",
                IsNSFW = isNSFW,
                IsSpoiler = isSpoiler,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            post.GenerateSlug();
            return post;
        }

        /// <summary>
        /// Creates a new link post
        /// </summary>
        public Post CreateLinkPost(string title, string url, int communityId, string? content = null, string? userId = null,
            bool isNSFW = false, bool isSpoiler = false)
        {
            var post = new Post
            {
                Title = title,
                ExternalUrl = url,
                Content = content,
                CommunityId = communityId,
                UserId = userId ?? CurrentUserId,
                PostType = "link",
                IsNSFW = isNSFW,
                IsSpoiler = isSpoiler,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            post.GenerateSlug();
            return post;
        }

        /// <summary>
        /// Creates a new image post
        /// </summary>
        public Post CreateImagePost(string title, int communityId, string? content = null, string? userId = null,
            bool isNSFW = false, bool isSpoiler = false)
        {
            var post = new Post
            {
                Title = title,
                Content = content,
                CommunityId = communityId,
                UserId = userId ?? CurrentUserId,
                PostType = "image",
                IsNSFW = isNSFW,
                IsSpoiler = isSpoiler,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            post.GenerateSlug();
            return post;
        }

        /// <summary>
        /// Creates a new video post
        /// </summary>
        public Post CreateVideoPost(string title, int communityId, string? content = null, string? userId = null,
            bool isNSFW = false, bool isSpoiler = false)
        {
            var post = new Post
            {
                Title = title,
                Content = content,
                CommunityId = communityId,
                UserId = userId ?? CurrentUserId,
                PostType = "video",
                IsNSFW = isNSFW,
                IsSpoiler = isSpoiler,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            post.GenerateSlug();
            return post;
        }

        /// <summary>
        /// Creates a new poll post
        /// </summary>
        public Post CreatePollPost(string title, int communityId, List<string> options, string? content = null,
            string? userId = null, bool isNSFW = false, bool isSpoiler = false, bool allowMultipleChoices = false,
            DateTime? endDate = null, bool showResultsBeforeVoting = true)
        {
            var post = new Post
            {
                Title = title,
                Content = content,
                CommunityId = communityId,
                UserId = userId ?? CurrentUserId,
                PostType = "poll",
                IsNSFW = isNSFW,
                IsSpoiler = isSpoiler,
                Status = "published",
                HasPoll = true,
                PollOptionCount = options.Count,
                PollVoteCount = 0,
                PollExpiresAt = endDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            post.GenerateSlug();
            return post;
        }

        /// <summary>
        /// Creates a poll configuration for a post
        /// </summary>
        public PollConfiguration CreatePollConfiguration(int postId, bool allowMultipleChoices = false,
            DateTime? endDate = null, bool showResultsBeforeVoting = true, bool showResultsBeforeEnd = true,
            bool allowAddingOptions = false, int minOptions = 2, int maxOptions = 10)
        {
            return new PollConfiguration
            {
                PostId = postId,
                AllowMultipleChoices = allowMultipleChoices,
                EndDate = endDate,
                ShowResultsBeforeVoting = showResultsBeforeVoting,
                ShowResultsBeforeEnd = showResultsBeforeEnd,
                AllowAddingOptions = allowAddingOptions,
                MinOptions = minOptions,
                MaxOptions = maxOptions,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a poll option
        /// </summary>
        public PollOption CreatePollOption(int postId, string optionText, int displayOrder = 0)
        {
            return new PollOption
            {
                PostId = postId,
                OptionText = optionText,
                DisplayOrder = displayOrder,
                VoteCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a post vote
        /// </summary>
        public PostVote CreatePostVote(int postId, string? userId = null, int voteType = 1)
        {
            return new PostVote
            {
                PostId = postId,
                UserId = userId ?? CurrentUserId ?? throw new InvalidOperationException("User ID is required"),
                VoteType = voteType,
                VotedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a poll vote
        /// </summary>
        public PollVote CreatePollVote(int pollOptionId, string? userId = null)
        {
            return new PollVote
            {
                PollOptionId = pollOptionId,
                UserId = userId ?? CurrentUserId ?? throw new InvalidOperationException("User ID is required"),
                VotedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Comment Methods

        /// <summary>
        /// Creates a new comment on a post
        /// </summary>
        public Comment CreateComment(int postId, string content, string? userId = null, int? parentCommentId = null)
        {
            // Calculate tree level based on parent comment
            int treeLevel = 0;
            if (parentCommentId.HasValue)
            {
                var parentComment = _dbContext.Comments.Find(parentCommentId.Value);
                if (parentComment != null)
                {
                    treeLevel = parentComment.TreeLevel + 1;
                }
            }

            return new Comment
            {
                PostId = postId,
                Content = content,
                UserId = userId ?? CurrentUserId,
                ParentCommentId = parentCommentId,
                TreeLevel = treeLevel,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a comment vote
        /// </summary>
        public CommentVote CreateCommentVote(int commentId, string? userId = null, int voteType = 1)
        {
            return new CommentVote
            {
                CommentId = commentId,
                UserId = userId ?? CurrentUserId ?? throw new InvalidOperationException("User ID is required"),
                VoteType = voteType,
                VotedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Media Methods

        /// <summary>
        /// Creates a new media item
        /// </summary>
        public Media CreateMedia(string url, string mediaType, string? thumbnailUrl = null, int? postId = null,
            string? userId = null, string? contentType = null, string? fileName = null, long? fileSize = null,
            int? width = null, int? height = null, int? duration = null, string? caption = null,
            string? altText = null, string storageProvider = "local", string? storagePath = null)
        {
            return new Media
            {
                Url = url,
                ThumbnailUrl = thumbnailUrl,
                MediaType = mediaType,
                PostId = postId,
                UserId = userId ?? CurrentUserId,
                ContentType = contentType,
                FileName = fileName,
                FileSize = fileSize,
                Width = width,
                Height = height,
                Duration = duration,
                Caption = caption,
                AltText = altText,
                StorageProvider = storageProvider,
                StoragePath = storagePath,
                UploadedAt = DateTime.UtcNow,
                IsProcessed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Tag Methods

        /// <summary>
        /// Creates a new tag
        /// </summary>
        public Tag CreateTag(string name, string? description = null)
        {
            var tag = new Tag
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            tag.GenerateSlug();
            return tag;
        }

        /// <summary>
        /// Creates a post tag
        /// </summary>
        public PostTag CreatePostTag(int postId, int tagId)
        {
            return new PostTag
            {
                PostId = postId,
                TagId = tagId
            };
        }

        #endregion

        #region SEO Methods

        /// <summary>
        /// Creates SEO metadata for a post
        /// </summary>
        public SeoMetadata CreatePostSeoMetadata(int postId, string? metaTitle = null, string? metaDescription = null,
            string? canonicalUrl = null, string? ogTitle = null, string? ogDescription = null, string? ogImageUrl = null,
            string twitterCard = "summary", string? twitterTitle = null, string? twitterDescription = null,
            string? twitterImageUrl = null, string? keywords = null, string? structuredData = null)
        {
            var seo = new SeoMetadata
            {
                EntityType = "post",
                EntityId = postId,
                MetaTitle = metaTitle,
                MetaDescription = metaDescription,
                CanonicalUrl = canonicalUrl,
                OgTitle = ogTitle,
                OgDescription = ogDescription,
                OgImageUrl = ogImageUrl,
                TwitterCard = twitterCard,
                TwitterTitle = twitterTitle,
                TwitterDescription = twitterDescription,
                TwitterImageUrl = twitterImageUrl,
                Keywords = keywords,
                StructuredData = structuredData,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If post exists, auto-generate SEO data
            var post = _dbContext.Posts.Find(postId);
            if (post != null)
            {
                seo.GenerateFromPost(post);
            }

            return seo;
        }

        /// <summary>
        /// Creates SEO metadata for a community
        /// </summary>
        public SeoMetadata CreateCommunitySeoMetadata(int communityId, string? metaTitle = null, string? metaDescription = null,
            string? canonicalUrl = null, string? ogTitle = null, string? ogDescription = null, string? ogImageUrl = null,
            string twitterCard = "summary", string? twitterTitle = null, string? twitterDescription = null,
            string? twitterImageUrl = null, string? keywords = null, string? structuredData = null)
        {
            var seo = new SeoMetadata
            {
                EntityType = "community",
                EntityId = communityId,
                MetaTitle = metaTitle,
                MetaDescription = metaDescription,
                CanonicalUrl = canonicalUrl,
                OgTitle = ogTitle,
                OgDescription = ogDescription,
                OgImageUrl = ogImageUrl,
                TwitterCard = twitterCard,
                TwitterTitle = twitterTitle,
                TwitterDescription = twitterDescription,
                TwitterImageUrl = twitterImageUrl,
                Keywords = keywords,
                StructuredData = structuredData,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If community exists, auto-generate SEO data
            var community = _dbContext.Communities.Find(communityId);
            if (community != null)
            {
                seo.GenerateFromCommunity(community);
            }

            return seo;
        }

        #endregion
    }
}
