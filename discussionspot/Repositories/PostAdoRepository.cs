using discussionspot.Models.Domain;
using Microsoft.Data.SqlClient;

namespace discussionspot.Repositories
{
    /// <summary>
    /// ADO.NET Repository for Posts with optimized direct SQL access
    /// </summary>
    public class PostAdoRepository : AdoRepository<Post>
    {
        public PostAdoRepository(IConfiguration configuration)
            : base(configuration, "Post", "PostId")
        {
        }

        /// <summary>
        /// Gets popular posts with optimized SQL query
        /// </summary>
        /// <param name="communityId">Optional community ID to filter by</param>
        /// <param name="days">Number of days to look back</param>
        /// <param name="pageSize">Number of posts to return</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <returns>List of popular posts</returns>
        public async Task<List<Post>> GetPopularPostsAsync(int? communityId = null, int days = 7, int pageSize = 20, int pageNumber = 1)
        {
            var posts = new List<Post>();
            int skip = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT p.*, u.UserName, u.Email, c.Name as CommunityName, c.Slug as CommunitySlug, 
                       (SELECT COUNT(*) FROM Comment WHERE PostId = p.PostId) as CommentCountActual,
                       (SELECT COUNT(*) FROM Media WHERE PostId = p.PostId) as MediaCount
                FROM Post p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                LEFT JOIN Community c ON p.CommunityId = c.CommunityId
                WHERE p.CreatedAt >= @StartDate
                AND p.Status = 'published'
                AND (@CommunityId IS NULL OR p.CommunityId = @CommunityId)
                ORDER BY (p.UpvoteCount - p.DownvoteCount) DESC, p.CommentCount DESC, p.CreatedAt DESC
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@StartDate", DateTime.UtcNow.AddDays(-days));
                command.Parameters.AddWithValue("@Skip", skip);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                if (communityId.HasValue)
                    command.Parameters.AddWithValue("@CommunityId", communityId.Value);
                else
                    command.Parameters.AddWithValue("@CommunityId", DBNull.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var post = MapEntityFromReader(reader);

                        // Add additional data from the query
                        if (post != null)
                        {
                            // Override comment count with actual count (in case they're out of sync)
                            if (!reader.IsDBNull(reader.GetOrdinal("CommentCountActual")))
                                post.CommentCount = reader.GetInt32(reader.GetOrdinal("CommentCountActual"));

                            posts.Add(post);
                        }
                    }
                }
            }

            return posts;
        }

        /// <summary>
        /// Gets latest posts with optimized SQL query
        /// </summary>
        /// <param name="communityId">Optional community ID to filter by</param>
        /// <param name="pageSize">Number of posts to return</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <returns>List of latest posts</returns>
        public async Task<List<Post>> GetLatestPostsAsync(int? communityId = null, int pageSize = 20, int pageNumber = 1)
        {
            var posts = new List<Post>();
            int skip = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT p.*, u.UserName, u.Email, c.Name as CommunityName, c.Slug as CommunitySlug,
                       (SELECT COUNT(*) FROM Media WHERE PostId = p.PostId) as MediaCount
                FROM Post p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                LEFT JOIN Community c ON p.CommunityId = c.CommunityId
                WHERE p.Status = 'published'
                AND (@CommunityId IS NULL OR p.CommunityId = @CommunityId)
                ORDER BY p.CreatedAt DESC
                OFFSET @Skip ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Skip", skip);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                if (communityId.HasValue)
                    command.Parameters.AddWithValue("@CommunityId", communityId.Value);
                else
                    command.Parameters.AddWithValue("@CommunityId", DBNull.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var post = MapEntityFromReader(reader);
                        if (post != null)
                        {
                            posts.Add(post);
                        }
                    }
                }
            }

            return posts;
        }

        /// <summary>
        /// Gets post with all related data in a single optimized query
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>Post with related data</returns>
        public async Task<Post?> GetPostWithDetailsAsync(int postId)
        {
            Post? post = null;

            var sql = @"
                SELECT p.*, u.UserName, u.Email, c.Name as CommunityName, c.Slug as CommunitySlug, c.IconUrl as CommunityIconUrl,
                       (SELECT COUNT(*) FROM Media WHERE PostId = p.PostId) as MediaCount,
                       (SELECT COUNT(*) FROM Comment WHERE PostId = p.PostId) as CommentCountActual
                FROM Post p
                LEFT JOIN AspNetUsers u ON p.UserId = u.Id
                LEFT JOIN Community c ON p.CommunityId = c.CommunityId
                WHERE p.PostId = @PostId
                AND p.Status = 'published'";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        post = MapEntityFromReader(reader);

                        // Update comment count with actual count
                        if (post != null && !reader.IsDBNull(reader.GetOrdinal("CommentCountActual")))
                        {
                            post.CommentCount = reader.GetInt32(reader.GetOrdinal("CommentCountActual"));
                        }
                    }
                }

                // If post was found, load associated data
                if (post != null)
                {
                    // Load media
                    post.Media = await GetPostMediaAsync(postId);

                    // Load poll options if it's a poll
                    if (post.HasPoll)
                    {
                        post.PollOptions = await GetPollOptionsAsync(postId);
                        post.PollConfiguration = await GetPollConfigurationAsync(postId);
                    }

                    // Load tags
                    var tags = await GetPostTagsAsync(postId);
                    post.PostTags = tags;

                    // Load SEO metadata
                    post.SeoMetadata = await GetPostSeoMetadataAsync(postId);
                }
            }

            return post;
        }

        /// <summary>
        /// Gets media items for a post
        /// </summary>
        private async Task<List<Media>> GetPostMediaAsync(int postId)
        {
            var media = new List<Media>();

            var sql = "SELECT * FROM Media WHERE PostId = @PostId";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        media.Add(MapMediaFromReader(reader));
                    }
                }
            }

            return media;
        }

        /// <summary>
        /// Gets poll options for a post
        /// </summary>
        private async Task<List<PollOption>> GetPollOptionsAsync(int postId)
        {
            var options = new List<PollOption>();

            var sql = "SELECT * FROM PollOption WHERE PostId = @PostId ORDER BY DisplayOrder";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        options.Add(MapPollOptionFromReader(reader));
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Gets poll configuration for a post
        /// </summary>
        private async Task<PollConfiguration?> GetPollConfigurationAsync(int postId)
        {
            PollConfiguration? config = null;

            var sql = "SELECT * FROM PollConfiguration WHERE PostId = @PostId";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        config = MapPollConfigurationFromReader(reader);
                    }
                }
            }

            return config;
        }

        /// <summary>
        /// Gets tags for a post
        /// </summary>
        private async Task<List<PostTag>> GetPostTagsAsync(int postId)
        {
            var tags = new List<PostTag>();

            var sql = @"
                SELECT pt.*, t.Name, t.Slug, t.Description
                FROM PostTag pt
                INNER JOIN Tag t ON pt.TagId = t.TagId
                WHERE pt.PostId = @PostId";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var postTag = new PostTag
                        {
                            PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                            TagId = reader.GetInt32(reader.GetOrdinal("TagId")),
                            Tag = new Tag
                            {
                                TagId = reader.GetInt32(reader.GetOrdinal("TagId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Slug = reader.GetString(reader.GetOrdinal("Slug")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                            }
                        };

                        tags.Add(postTag);
                    }
                }
            }

            return tags;
        }

        /// <summary>
        /// Gets SEO metadata for a post
        /// </summary>
        private async Task<SeoMetadata?> GetPostSeoMetadataAsync(int postId)
        {
            SeoMetadata? metadata = null;

            var sql = "SELECT * FROM SeoMetadata WHERE EntityType = 'post' AND EntityId = @PostId";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PostId", postId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        metadata = MapSeoMetadataFromReader(reader);
                    }
                }
            }

            return metadata;
        }

        #region Helper Methods for Mapping Entities

        /// <summary>
        /// Maps a data reader to a post entity
        /// </summary>
        protected override Post MapEntityFromReader(SqlDataReader reader)
        {
            var post = new Post
            {
                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Slug = reader.GetString(reader.GetOrdinal("Slug")),
                Content = reader.IsDBNull(reader.GetOrdinal("Content")) ? null : reader.GetString(reader.GetOrdinal("Content")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
                CommunityId = reader.GetInt32(reader.GetOrdinal("CommunityId")),
                PostType = reader.GetString(reader.GetOrdinal("PostType")),
                ExternalUrl = reader.IsDBNull(reader.GetOrdinal("Url")) ? null : reader.GetString(reader.GetOrdinal("Url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                UpvoteCount = reader.GetInt32(reader.GetOrdinal("UpvoteCount")),
                DownvoteCount = reader.GetInt32(reader.GetOrdinal("DownvoteCount")),
                CommentCount = reader.GetInt32(reader.GetOrdinal("CommentCount")),
                Score = reader.GetInt32(reader.GetOrdinal("Score")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                IsPinned = reader.GetBoolean(reader.GetOrdinal("IsPinned")),
                IsLocked = reader.GetBoolean(reader.GetOrdinal("IsLocked")),
                IsNSFW = reader.GetBoolean(reader.GetOrdinal("IsNSFW")),
                IsSpoiler = reader.GetBoolean(reader.GetOrdinal("IsSpoiler")),
                ViewCount = reader.GetInt32(reader.GetOrdinal("ViewCount")),
                HasPoll = reader.GetBoolean(reader.GetOrdinal("HasPoll")),
                PollOptionCount = reader.GetInt32(reader.GetOrdinal("PollOptionCount")),
                PollVoteCount = reader.GetInt32(reader.GetOrdinal("PollVoteCount")),
                PollExpiresAt = reader.IsDBNull(reader.GetOrdinal("PollExpiresAt")) ? null : reader.GetDateTime(reader.GetOrdinal("PollExpiresAt"))
            };

            // Add user details if columns exist in the result set
            if (HasColumn(reader, "UserName") && !reader.IsDBNull(reader.GetOrdinal("UserName")))
            {
                post.User = new ApplicationUsers
                {
                    Id = post.UserId,
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email"))
                };
            }

            // Add community details if columns exist in the result set
            if (HasColumn(reader, "CommunityName") && !reader.IsDBNull(reader.GetOrdinal("CommunityName")))
            {
                post.Community = new Community
                {
                    CommunityId = post.CommunityId,
                    Name = reader.GetString(reader.GetOrdinal("CommunityName")),
                    Slug = reader.GetString(reader.GetOrdinal("CommunitySlug")),
                    IconUrl = HasColumn(reader, "CommunityIconUrl") && !reader.IsDBNull(reader.GetOrdinal("CommunityIconUrl")) ?
                              reader.GetString(reader.GetOrdinal("CommunityIconUrl")) : null
                };
            }

            return post;
        }

        /// <summary>
        /// Maps a data reader to a media entity
        /// </summary>
        private Media MapMediaFromReader(SqlDataReader reader)
        {
            return new Media
            {
                MediaId = reader.GetInt32(reader.GetOrdinal("MediaId")),
                Url = reader.GetString(reader.GetOrdinal("Url")),
                ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl")) ? null : reader.GetString(reader.GetOrdinal("ThumbnailUrl")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
                PostId = reader.IsDBNull(reader.GetOrdinal("PostId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("PostId")),
                MediaType = reader.GetString(reader.GetOrdinal("MediaType")),
                ContentType = reader.IsDBNull(reader.GetOrdinal("ContentType")) ? null : reader.GetString(reader.GetOrdinal("ContentType")),
                FileName = reader.IsDBNull(reader.GetOrdinal("FileName")) ? null : reader.GetString(reader.GetOrdinal("FileName")),
                FileSize = reader.IsDBNull(reader.GetOrdinal("FileSize")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("FileSize")),
                Width = reader.IsDBNull(reader.GetOrdinal("Width")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Width")),
                Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Height")),
                Duration = reader.IsDBNull(reader.GetOrdinal("Duration")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Duration")),
                Caption = reader.IsDBNull(reader.GetOrdinal("Caption")) ? null : reader.GetString(reader.GetOrdinal("Caption")),
                AltText = reader.IsDBNull(reader.GetOrdinal("AltText")) ? null : reader.GetString(reader.GetOrdinal("AltText")),
                UploadedAt = reader.GetDateTime(reader.GetOrdinal("UploadedAt")),
                StorageProvider = reader.GetString(reader.GetOrdinal("StorageProvider")),
                StoragePath = reader.IsDBNull(reader.GetOrdinal("StoragePath")) ? null : reader.GetString(reader.GetOrdinal("StoragePath")),
                IsProcessed = reader.GetBoolean(reader.GetOrdinal("IsProcessed")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        /// <summary>
        /// Maps a data reader to a poll option entity
        /// </summary>
        private PollOption MapPollOptionFromReader(SqlDataReader reader)
        {
            return new PollOption
            {
                PollOptionId = reader.GetInt32(reader.GetOrdinal("PollOptionId")),
                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                OptionText = reader.GetString(reader.GetOrdinal("OptionText")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                VoteCount = reader.GetInt32(reader.GetOrdinal("VoteCount")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        /// <summary>
        /// Maps a data reader to a poll configuration entity
        /// </summary>
        private PollConfiguration MapPollConfigurationFromReader(SqlDataReader reader)
        {
            return new PollConfiguration
            {
                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                AllowMultipleChoices = reader.GetBoolean(reader.GetOrdinal("AllowMultipleChoices")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EndDate")),
                ShowResultsBeforeVoting = reader.GetBoolean(reader.GetOrdinal("ShowResultsBeforeVoting")),
                ShowResultsBeforeEnd = reader.GetBoolean(reader.GetOrdinal("ShowResultsBeforeEnd")),
                AllowAddingOptions = reader.GetBoolean(reader.GetOrdinal("AllowAddingOptions")),
                MinOptions = reader.GetInt32(reader.GetOrdinal("MinOptions")),
                MaxOptions = reader.GetInt32(reader.GetOrdinal("MaxOptions")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        /// <summary>
        /// Maps a data reader to a SEO metadata entity
        /// </summary>
        private SeoMetadata MapSeoMetadataFromReader(SqlDataReader reader)
        {
            return new SeoMetadata
            {
                EntityType = reader.GetString(reader.GetOrdinal("EntityType")),
                EntityId = reader.GetInt32(reader.GetOrdinal("EntityId")),
                MetaTitle = reader.IsDBNull(reader.GetOrdinal("MetaTitle")) ? null : reader.GetString(reader.GetOrdinal("MetaTitle")),
                MetaDescription = reader.IsDBNull(reader.GetOrdinal("MetaDescription")) ? null : reader.GetString(reader.GetOrdinal("MetaDescription")),
                CanonicalUrl = reader.IsDBNull(reader.GetOrdinal("CanonicalUrl")) ? null : reader.GetString(reader.GetOrdinal("CanonicalUrl")),
                OgTitle = reader.IsDBNull(reader.GetOrdinal("OgTitle")) ? null : reader.GetString(reader.GetOrdinal("OgTitle")),
                OgDescription = reader.IsDBNull(reader.GetOrdinal("OgDescription")) ? null : reader.GetString(reader.GetOrdinal("OgDescription")),
                OgImageUrl = reader.IsDBNull(reader.GetOrdinal("OgImageUrl")) ? null : reader.GetString(reader.GetOrdinal("OgImageUrl")),
                TwitterCard = reader.GetString(reader.GetOrdinal("TwitterCard")),
                TwitterTitle = reader.IsDBNull(reader.GetOrdinal("TwitterTitle")) ? null : reader.GetString(reader.GetOrdinal("TwitterTitle")),
                TwitterDescription = reader.IsDBNull(reader.GetOrdinal("TwitterDescription")) ? null : reader.GetString(reader.GetOrdinal("TwitterDescription")),
                TwitterImageUrl = reader.IsDBNull(reader.GetOrdinal("TwitterImageUrl")) ? null : reader.GetString(reader.GetOrdinal("TwitterImageUrl")),
                Keywords = reader.IsDBNull(reader.GetOrdinal("Keywords")) ? null : reader.GetString(reader.GetOrdinal("Keywords")),
                StructuredData = reader.IsDBNull(reader.GetOrdinal("StructuredData")) ? null : reader.GetString(reader.GetOrdinal("StructuredData")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        /// <summary>
        /// Checks if a column exists in the data reader
        /// </summary>
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion

        #region ADO.NET Repository Implementation

        /// <summary>
        /// Generates an INSERT command for a post
        /// </summary>
        protected override (string sql, List<SqlParameter> parameters) GenerateInsertCommand(Post post)
        {
            var parameters = new List<SqlParameter>();

            var sql = @"
                INSERT INTO Post (
                    Title, Slug, Content, UserId, CommunityId, PostType, Url, CreatedAt, UpdatedAt,
                    UpvoteCount, DownvoteCount, CommentCount, Score, Status, IsPinned, IsLocked,
                    IsNSFW, IsSpoiler, ViewCount, HasPoll, PollOptionCount, PollVoteCount, PollExpiresAt
                ) VALUES (
                    @Title, @Slug, @Content, @UserId, @CommunityId, @PostType, @Url, @CreatedAt, @UpdatedAt,
                    @UpvoteCount, @DownvoteCount, @CommentCount, @Score, @Status, @IsPinned, @IsLocked,
                    @IsNSFW, @IsSpoiler, @ViewCount, @HasPoll, @PollOptionCount, @PollVoteCount, @PollExpiresAt
                );
                SELECT SCOPE_IDENTITY();";

            parameters.Add(new SqlParameter("@Title", post.Title));
            parameters.Add(new SqlParameter("@Slug", post.Slug));
            parameters.Add(new SqlParameter("@Content", (object?)post.Content ?? DBNull.Value));
            parameters.Add(new SqlParameter("@UserId", (object?)post.UserId ?? DBNull.Value));
            parameters.Add(new SqlParameter("@CommunityId", post.CommunityId));
            parameters.Add(new SqlParameter("@PostType", post.PostType));
            parameters.Add(new SqlParameter("@Url", (object?)post.ExternalUrl ?? DBNull.Value));
            parameters.Add(new SqlParameter("@CreatedAt", post.CreatedAt));
            parameters.Add(new SqlParameter("@UpdatedAt", post.UpdatedAt));
            parameters.Add(new SqlParameter("@UpvoteCount", post.UpvoteCount));
            parameters.Add(new SqlParameter("@DownvoteCount", post.DownvoteCount));
            parameters.Add(new SqlParameter("@CommentCount", post.CommentCount));
            parameters.Add(new SqlParameter("@Score", post.Score));
            parameters.Add(new SqlParameter("@Status", post.Status));
            parameters.Add(new SqlParameter("@IsPinned", post.IsPinned));
            parameters.Add(new SqlParameter("@IsLocked", post.IsLocked));
            parameters.Add(new SqlParameter("@IsNSFW", post.IsNSFW));
            parameters.Add(new SqlParameter("@IsSpoiler", post.IsSpoiler));
            parameters.Add(new SqlParameter("@ViewCount", post.ViewCount));
            parameters.Add(new SqlParameter("@HasPoll", post.HasPoll));
            parameters.Add(new SqlParameter("@PollOptionCount", post.PollOptionCount));
            parameters.Add(new SqlParameter("@PollVoteCount", post.PollVoteCount));
            parameters.Add(new SqlParameter("@PollExpiresAt", (object?)post.PollExpiresAt ?? DBNull.Value));

            return (sql, parameters);
        }

        /// <summary>
        /// Generates an UPDATE command for a post
        /// </summary>
        protected override (string sql, List<SqlParameter> parameters) GenerateUpdateCommand(Post post)
        {
            var parameters = new List<SqlParameter>();

            var sql = @"
                UPDATE Post SET
                    Title = @Title,
                    Slug = @Slug,
                    Content = @Content,
                    UserId = @UserId,
                    CommunityId = @CommunityId,
                    PostType = @PostType,
                    Url = @Url,
                    UpdatedAt = @UpdatedAt,
                    UpvoteCount = @UpvoteCount,
                    DownvoteCount = @DownvoteCount,
                    CommentCount = @CommentCount,
                    Score = @Score,
                    Status = @Status,
                    IsPinned = @IsPinned,
                    IsLocked = @IsLocked,
                    IsNSFW = @IsNSFW,
                    IsSpoiler = @IsSpoiler,
                    ViewCount = @ViewCount,
                    HasPoll = @HasPoll,
                    PollOptionCount = @PollOptionCount,
                    PollVoteCount = @PollVoteCount,
                    PollExpiresAt = @PollExpiresAt
                WHERE PostId = @PostId";

            parameters.Add(new SqlParameter("@PostId", post.PostId));
            parameters.Add(new SqlParameter("@Title", post.Title));
            parameters.Add(new SqlParameter("@Slug", post.Slug));
            parameters.Add(new SqlParameter("@Content", (object?)post.Content ?? DBNull.Value));
            parameters.Add(new SqlParameter("@UserId", (object?)post.UserId ?? DBNull.Value));
            parameters.Add(new SqlParameter("@CommunityId", post.CommunityId));
            parameters.Add(new SqlParameter("@PostType", post.PostType));
            parameters.Add(new SqlParameter("@Url", (object?)post.ExternalUrl ?? DBNull.Value));
            parameters.Add(new SqlParameter("@UpdatedAt", DateTime.UtcNow));
            parameters.Add(new SqlParameter("@UpvoteCount", post.UpvoteCount));
            parameters.Add(new SqlParameter("@DownvoteCount", post.DownvoteCount));
            parameters.Add(new SqlParameter("@CommentCount", post.CommentCount));
            parameters.Add(new SqlParameter("@Score", post.Score));
            parameters.Add(new SqlParameter("@Status", post.Status));
            parameters.Add(new SqlParameter("@IsPinned", post.IsPinned));
            parameters.Add(new SqlParameter("@IsLocked", post.IsLocked));
            parameters.Add(new SqlParameter("@IsNSFW", post.IsNSFW));
            parameters.Add(new SqlParameter("@IsSpoiler", post.IsSpoiler));
            parameters.Add(new SqlParameter("@ViewCount", post.ViewCount));
            parameters.Add(new SqlParameter("@HasPoll", post.HasPoll));
            parameters.Add(new SqlParameter("@PollOptionCount", post.PollOptionCount));
            parameters.Add(new SqlParameter("@PollVoteCount", post.PollVoteCount));
            parameters.Add(new SqlParameter("@PollExpiresAt", (object?)post.PollExpiresAt ?? DBNull.Value));

            return (sql, parameters);
        }

        #endregion
    }
}
