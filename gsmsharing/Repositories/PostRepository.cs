using gsmsharing.Database;
using gsmsharing.ExeMethods;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.Models.SEO;
using gsmsharing.ViewModels;
using System.Data;

namespace gsmsharing.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly PostSEODataAccess _postSEODataAccess;
        private readonly DatabaseConnection _db;

        public PostRepository(PostSEODataAccess postSEODataAccess, DatabaseConnection db)
        {
            _postSEODataAccess = postSEODataAccess;
            _db = db;
        }

        public async Task<int> CreateArticleSchema(int postId, string title, string authorName)
        {
            var schemaProperties = new Dictionary<string, object>
                {
                    { "headline", title },
                    { "datePublished", DateTime.UtcNow.ToString("o") },
                    { "dateModified", DateTime.UtcNow.ToString("o") },
                    { "author", new Dictionary<string, object>
                        {
                            { "@type", "Person" },
                            { "name", authorName }
                        }
                    }
                };

            return await _postSEODataAccess.InsertSchemaData(postId,
                PostSEODataAccess.SchemaType.Article,
                schemaProperties);
        }

        public async Task<int> CreateAsync(Post post)
        {
            try
            {
                String slugs = SlugGenerator.GenerateSlug(post.Slug);
                post.Slug = slugs;
                const string sql = @"
                        INSERT INTO Posts (
                            UserId, Title, Slug, Content, FeaturedImage,
                            PostStatus, AllowComments, 
                            CommunityID, IsPromoted, IsFeatured
                        )
                        VALUES (
                            @UserId, @Title, @Slug, @Content, @FeaturedImage,
                            @PostStatus, @AllowComments, 
                            @CommunityID,  @IsPromoted, @IsFeatured
                        );
                        SELECT SCOPE_IDENTITY();";


                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", post.UserId },
                    { "@Title", post.Title },
                    { "@Slug", post.Slug },
                    { "@Content", post.Content },
                    { "@FeaturedImage", (object)post.FeaturedImage ?? DBNull.Value },
                    { "@PostStatus", post.PostStatus ?? "draft" },
                    { "@AllowComments", post.AllowComments ?? true },
                    { "@CommunityID", (object)post.CommunityID ?? DBNull.Value },
                    { "@IsPromoted",(object)post.IsPromoted ?? DBNull.Value},
                    {"@IsFeatured", (object)post.IsFeatured ?? DBNull.Value }
                };

                return await _db.ExecuteScalarAsync<int>(sql, parameters);
            }

            catch (Exception ex)
            {
                ex.Message.ToString();
                throw;
            }

        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                const string sql = @"DELETE FROM Posts WHERE PostID = @PostID; SELECT @@ROWCOUNT;";
                
                var parameters = new Dictionary<string, object>
                {
                    { "@PostID", id }
                };

                await _db.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting post: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PostViewModelDisplay>> GetAllAsync()
        {
            try
            {
                const string sql = @"
        SELECT 
            p.[PostID],
            p.[Title],
            p.[Slug],
            p.[FeaturedImage],
            p.[ViewCount],
            s.[MetaDescription],
            s.[MetaKeywords],
            p.[CreatedAt],
            p.[PublishedAt],
            u.[UserName] AS AuthorName,
            c.[Name] AS CommunityName,
            c.[Slug] AS CommunitySlug,
            (SELECT COALESCE(SUM(PostLikes), 0) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalLikes,
            (SELECT COALESCE(SUM(PostDisLikes), 0) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalDislikes,
            (SELECT COUNT(*) FROM Comments cm WHERE cm.PostID = p.PostID AND cm.IsApproved = 1) AS TotalComments
        FROM Posts p
        LEFT JOIN PostSEO s ON p.[PostID] = s.[PostID]
        LEFT JOIN AspNetUsers u ON p.[UserId] = u.[Id]
        LEFT JOIN Communities c ON p.[CommunityID] = c.[CommunityID]
        ORDER BY p.[CreatedAt] DESC";

                var dataTable = await _db.ExecuteQueryAsync(sql);
                var posts = new List<PostViewModelDisplay>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var totalLikes = Convert.ToInt32(row["TotalLikes"]);
                    var totalDislikes = Convert.ToInt32(row["TotalDislikes"]);
                    var totalComments = Convert.ToInt32(row["TotalComments"]);

                    var post = new PostViewModelDisplay
                    {
                        PostID = Convert.ToInt32(row["PostID"]),
                        Title = row["Title"].ToString(),
                        Slug = row["Slug"].ToString(),
                        FeaturedImage = row["FeaturedImage"] as string,
                        ViewCount = FormatViewCount(Convert.ToInt32(row["ViewCount"])),
                        Discription = row["MetaDescription"] as string,
                        Keywords = row["MetaKeywords"] as string,
                        AuthorName = row["AuthorName"].ToString(),
                        CommunityName = row["CommunityName"].ToString(),
                        CommunitySlug = row["CommunitySlug"].ToString(),
                        CreatedTime = FormatTimeAgo(row["CreatedAt"] as DateTime?),
                        PublishedTime = FormatTimeAgo(row["PublishedAt"] as DateTime?),
                        Reactions = new ReactionSummary
                        {
                            TotalReactions = totalLikes + totalDislikes,
                            LikeCount = totalLikes,
                            DislikeCount = totalDislikes,
                            FormattedLikeCount = FormatLikeCount(totalLikes),
                            FormattedDislikeCount = FormatLikeCount(totalDislikes)
                        },
                        CommentCount = FormatLikeCount(totalComments)
                    };
                    posts.Add(post);
                }

                return posts;
            }
            catch (Exception ex)
            {

                return Enumerable.Empty<PostViewModelDisplay>();
            }
        }

        public static string FormatViewCount(int viewCount)
        {
            if (viewCount < 1000)
                return viewCount.ToString();

            // Round to one decimal place for k notation
            double formattedCount = Math.Floor(viewCount / 100.0) / 10.0;
            return $"{formattedCount:0.#}k";
        }


        /// <summary>
        /// Converts like count to k notation for counts greater than 1000
        /// </summary>
        /// <param name="likeCount">Total number of likes</param>
        /// <returns>Formatted like count string</returns>
        public static string FormatLikeCount(int likeCount)
        {
            if (likeCount < 1000)
                return likeCount.ToString();

            // Round to one decimal place for k notation
            double formattedCount = Math.Floor(likeCount / 100.0) / 10.0;
            return $"{formattedCount:0.#}k";
        }

        // Helper method to format time ago
        private string FormatTimeAgo(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return "Not available";

            var now = DateTime.UtcNow;
            var diff = now - dateTime.Value;

            return diff.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)diff.TotalMinutes} minute{(diff.TotalMinutes > 1 ? "s" : "")} ago",
                < 1440 => $"{(int)diff.TotalHours} hour{(diff.TotalHours > 1 ? "s" : "")} ago",
                < 525600 => $"{(int)diff.TotalDays} day{(diff.TotalDays > 1 ? "s" : "")} ago",
                _ => $"{(int)(diff.TotalDays / 365)} year{(diff.TotalDays >= 730 ? "s" : "")} ago"
            };
        }

        public Task<IEnumerable<Post>> GetByCommunityIdAsync(int communityId)
        {
            throw new NotImplementedException();
        }

    
        public async Task<PostViewModelWithSEO> GetByIdAsync(int postId)
        {
            try
            {
                const string sql = @"
SELECT 
    p.[PostID],
    p.[UserId],
    p.[Title],
    p.[Slug],
    p.[Content],
    p.[FeaturedImage],
    p.[ViewCount],
    p.[PostStatus],
    p.[IsPromoted],
    p.[IsFeatured],
    p.[AllowComments],
    p.[CreatedAt],
    p.[UpdatedAt],
    p.[PublishedAt],
    p.[CommunityID],
    s.[SEOId],
    s.[MetaTitle],
    s.[MetaDescription],
    s.[MetaKeywords],
    s.[OgTitle],
    s.[OgDescription],
    s.[OgImage],
    s.[TwitterCard],
    s.[TwitterTitle],
    s.[TwitterDescription],
    s.[TwitterImage],
    s.[CanonicalURL],
    s.[Robots],
    s.[Schema],
    u.[UserName] AS AuthorName,
    c.[Name] AS CommunityName,
    c.[Slug] AS CommunitySlug,
    (SELECT COALESCE(SUM(PostLikes), 0) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalLikes,
    (SELECT COALESCE(SUM(PostDisLikes), 0) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalDislikes,
    (SELECT COUNT(*) FROM Comments cm WHERE cm.PostID = p.PostID AND cm.IsApproved = 1) AS TotalComments
FROM Posts p
LEFT JOIN PostSEO s ON p.[PostID] = s.[PostID]
LEFT JOIN AspNetUsers u ON p.[UserId] = u.[Id]
LEFT JOIN Communities c ON p.[CommunityID] = c.[CommunityID]
WHERE p.[PostID] = @PostId";

                var parameters = new Dictionary<string, object>
                {
                    { "@PostId", postId }
                };

                // Get the DataTable from the database
                var dataTable = await _db.ExecuteQueryAsync(sql, parameters);

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    return null;
                }

                var row = dataTable.Rows[0];
                var totalLikes = Convert.ToInt32(row["TotalLikes"]);
                var totalDislikes = Convert.ToInt32(row["TotalDislikes"]);
                var totalComments = Convert.ToInt32(row["TotalComments"]);

                var post = new PostViewModelWithSEO
                {
                    PostID = Convert.ToInt32(row["PostID"]),
                    UserId = row["UserId"].ToString(),
                    Title = row["Title"].ToString(),
                    Slug = row["Slug"].ToString(),
                    Content = row["Content"].ToString(),
                    FeaturedImage = row["FeaturedImage"] as string,
                    ViewCount = Convert.ToInt32(row["ViewCount"]),
                    FormattedViewCount = FormatViewCount(Convert.ToInt32(row["ViewCount"])),
                    PostStatus = row["PostStatus"].ToString(),
                    IsPromoted = Convert.ToBoolean(row["IsPromoted"]),
                    IsFeatured = Convert.ToBoolean(row["IsFeatured"]),
                    AllowComments = Convert.ToBoolean(row["AllowComments"]),
                    CreatedAt = row["CreatedAt"] as DateTime?,
                    UpdatedAt = row["UpdatedAt"] as DateTime?,
                    PublishedAt = row["PublishedAt"] as DateTime?,
                    CommunityID = Convert.ToInt32(row["CommunityID"]),
                    CreatedTime = FormatTimeAgo(row["CreatedAt"] as DateTime?),
                    PublishedTime = FormatTimeAgo(row["PublishedAt"] as DateTime?),
                    AuthorName = row["AuthorName"].ToString(),
                    CommunityName = row["CommunityName"].ToString(),
                    CommunitySlug = row["CommunitySlug"].ToString(),
                    Reactions = new ReactionSummary
                    {
                        TotalReactions = totalLikes + totalDislikes,
                        LikeCount = totalLikes,
                        DislikeCount = totalDislikes,
                        FormattedLikeCount = FormatLikeCount(totalLikes),
                        FormattedDislikeCount = FormatLikeCount(totalDislikes)
                    },
                    CommentCount = FormatLikeCount(totalComments),

                    // SEO Properties
                    SEOId = row["SEOId"] != DBNull.Value ? Convert.ToInt32(row["SEOId"]) : 0,
                    MetaTitle = row["MetaTitle"] as string,
                    MetaDescription = row["MetaDescription"] as string,
                    MetaKeywords = row["MetaKeywords"] as string,
                    OgTitle = row["OgTitle"] as string,
                    OgDescription = row["OgDescription"] as string,
                    OgImage = row["OgImage"] as string,
                    TwitterCard = row["TwitterCard"] as string,
                    TwitterTitle = row["TwitterTitle"] as string,
                    TwitterDescription = row["TwitterDescription"] as string,
                    TwitterImage = row["TwitterImage"] as string,
                    CanonicalURL = row["CanonicalURL"] as string,
                    Robots = row["Robots"] as string,
                    Schema = row["Schema"] as string
                };

                return post;
            }
            catch (Exception ex)
            {
                // Log the exception properly
                Console.WriteLine($"Error retrieving post with SEO data: {ex.Message}");
                return null;
            }
        }



        public async Task<Post> GetBySlugAsync(string slug)
        {
            try
            {
                const string sql = @"
                SELECT TOP 1
                    p.[PostID], p.[UserId], p.[Title], p.[Slug], p.[Content], p.[FeaturedImage],
                    p.[ViewCount], p.[PostStatus], p.[IsPromoted], p.[IsFeatured], p.[AllowComments],
                    p.[CreatedAt], p.[UpdatedAt], p.[PublishedAt], p.[CommunityID]
                FROM Posts p
                WHERE p.[Slug] = @Slug";

                var parameters = new Dictionary<string, object>
                {
                    { "@Slug", slug }
                };

                var dataTable = await _db.ExecuteQueryAsync(sql, parameters);
                
                if (dataTable == null || dataTable.Rows.Count == 0)
                    return null;

                var row = dataTable.Rows[0];
                return MapRowToPost(row);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving post by slug: {ex.Message}");
                return null;
            }
        }

        public async Task<PostViewModelWithSEO> GetBySlugAndCommunityAsync(string slug, string communitySlug)
        {
            try
            {
                const string sql = @"
SELECT 
    p.[PostID], p.[UserId], p.[Title], p.[Slug], p.[Content], p.[FeaturedImage],
    p.[ViewCount], p.[PostStatus], p.[IsPromoted], p.[IsFeatured], p.[AllowComments],
    p.[CreatedAt], p.[UpdatedAt], p.[PublishedAt], p.[CommunityID],
    s.[SEOId], s.[MetaTitle], s.[MetaDescription], s.[MetaKeywords],
    s.[OgTitle], s.[OgDescription], s.[OgImage],
    s.[TwitterCard], s.[TwitterTitle], s.[TwitterDescription], s.[TwitterImage],
    s.[CanonicalURL], s.[Robots], s.[Schema],
    u.[UserName] AS AuthorName,
    c.[Name] AS CommunityName,
    c.[Slug] AS CommunitySlug,
    (SELECT COALESCE(SUM(PostLikes), 0) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalLikes,
    (SELECT COALESCE(SUM(PostDisLikes), 0) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalDislikes,
    (SELECT COUNT(*) FROM Comments cm WHERE cm.PostID = p.PostID AND cm.IsApproved = 1) AS TotalComments
FROM Posts p
LEFT JOIN PostSEO s ON p.[PostID] = s.[PostID]
LEFT JOIN AspNetUsers u ON p.[UserId] = u.[Id]
LEFT JOIN Communities c ON p.[CommunityID] = c.[CommunityID]
WHERE p.[Slug] = @Slug AND c.[Slug] = @CommunitySlug";

                var parameters = new Dictionary<string, object>
                {
                    { "@Slug", slug },
                    { "@CommunitySlug", communitySlug }
                };

                var dataTable = await _db.ExecuteQueryAsync(sql, parameters);

                if (dataTable == null || dataTable.Rows.Count == 0)
                    return null;

                var row = dataTable.Rows[0];
                var totalLikes = Convert.ToInt32(row["TotalLikes"]);
                var totalDislikes = Convert.ToInt32(row["TotalDislikes"]);
                var totalComments = Convert.ToInt32(row["TotalComments"]);

                var post = new PostViewModelWithSEO
                {
                    PostID = Convert.ToInt32(row["PostID"]),
                    UserId = row["UserId"].ToString(),
                    Title = row["Title"].ToString(),
                    Slug = row["Slug"].ToString(),
                    Content = row["Content"].ToString(),
                    FeaturedImage = row["FeaturedImage"] as string,
                    ViewCount = Convert.ToInt32(row["ViewCount"]),
                    FormattedViewCount = FormatViewCount(Convert.ToInt32(row["ViewCount"])),
                    PostStatus = row["PostStatus"].ToString(),
                    IsPromoted = Convert.ToBoolean(row["IsPromoted"]),
                    IsFeatured = Convert.ToBoolean(row["IsFeatured"]),
                    AllowComments = Convert.ToBoolean(row["AllowComments"]),
                    CreatedAt = row["CreatedAt"] as DateTime?,
                    UpdatedAt = row["UpdatedAt"] as DateTime?,
                    PublishedAt = row["PublishedAt"] as DateTime?,
                    CommunityID = Convert.ToInt32(row["CommunityID"]),
                    CreatedTime = FormatTimeAgo(row["CreatedAt"] as DateTime?),
                    PublishedTime = FormatTimeAgo(row["PublishedAt"] as DateTime?),
                    AuthorName = row["AuthorName"].ToString(),
                    CommunityName = row["CommunityName"].ToString(),
                    CommunitySlug = row["CommunitySlug"].ToString(),
                    Reactions = new ReactionSummary
                    {
                        TotalReactions = totalLikes + totalDislikes,
                        LikeCount = totalLikes,
                        DislikeCount = totalDislikes,
                        FormattedLikeCount = FormatLikeCount(totalLikes),
                        FormattedDislikeCount = FormatLikeCount(totalDislikes)
                    },
                    CommentCount = FormatLikeCount(totalComments),
                    SEOId = row["SEOId"] != DBNull.Value ? Convert.ToInt32(row["SEOId"]) : 0,
                    MetaTitle = row["MetaTitle"] as string,
                    MetaDescription = row["MetaDescription"] as string,
                    MetaKeywords = row["MetaKeywords"] as string,
                    OgTitle = row["OgTitle"] as string,
                    OgDescription = row["OgDescription"] as string,
                    OgImage = row["OgImage"] as string,
                    TwitterCard = row["TwitterCard"] as string,
                    TwitterTitle = row["TwitterTitle"] as string,
                    TwitterDescription = row["TwitterDescription"] as string,
                    TwitterImage = row["TwitterImage"] as string,
                    CanonicalURL = row["CanonicalURL"] as string,
                    Robots = row["Robots"] as string,
                    Schema = row["Schema"] as string
                };

                return post;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving post by slug and community: {ex.Message}");
                return null;
            }
        }

        private Post MapRowToPost(DataRow row)
        {
            return new Post
            {
                PostID = Convert.ToInt32(row["PostID"]),
                UserId = row["UserId"].ToString(),
                Title = row["Title"].ToString(),
                Slug = row["Slug"].ToString(),
                Content = row["Content"].ToString(),
                FeaturedImage = row["FeaturedImage"] as string,
                ViewCount = Convert.ToInt32(row["ViewCount"]),
                PostStatus = row["PostStatus"].ToString(),
                IsPromoted = row["IsPromoted"] != DBNull.Value ? Convert.ToBoolean(row["IsPromoted"]) : false,
                IsFeatured = row["IsFeatured"] != DBNull.Value ? Convert.ToBoolean(row["IsFeatured"]) : false,
                AllowComments = row["AllowComments"] != DBNull.Value ? Convert.ToBoolean(row["AllowComments"]) : true,
                CreatedAt = row["CreatedAt"] as DateTime?,
                UpdatedAt = row["UpdatedAt"] as DateTime?,
                PublishedAt = row["PublishedAt"] as DateTime?,
                CommunityID = row["CommunityID"] != DBNull.Value ? Convert.ToInt32(row["CommunityID"]) : null
            };
        }

        public async Task<IEnumerable<Post>> GetByStatusAsync(string status)
        {
            try
            {
                const string sql = @"
                    SELECT 
                        p.[PostID], p.[UserId], p.[Title], p.[Slug], p.[Tags], p.[Content], 
                        p.[FeaturedImage], p.[PostStatus], p.[AllowComments], p.[IsPromoted], 
                        p.[IsFeatured], p.[ViewCount], p.[CreatedAt], p.[UpdatedAt], 
                        p.[PublishedAt], p.[CommunityID]
                    FROM Posts p
                    WHERE p.[PostStatus] = @Status
                    ORDER BY p.[CreatedAt] DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "@Status", status }
                };

                var dataTable = await _db.ExecuteQueryAsync(sql, parameters);
                var posts = new List<Post>();

                foreach (DataRow row in dataTable.Rows)
                {
                    posts.Add(MapRowToPost(row));
                }

                return posts;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving posts by status: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(string userId)
        {
            try
            {
                const string sql = @"
                    SELECT 
                        p.[PostID], p.[UserId], p.[Title], p.[Slug], p.[Tags], p.[Content], 
                        p.[FeaturedImage], p.[PostStatus], p.[AllowComments], p.[IsPromoted], 
                        p.[IsFeatured], p.[ViewCount], p.[CreatedAt], p.[UpdatedAt], 
                        p.[PublishedAt], p.[CommunityID],
                        c.[Name] AS CommunityName,
                        c.[Slug] AS CommunitySlug
                    FROM Posts p
                    LEFT JOIN Communities c ON p.[CommunityID] = c.[CommunityID]
                    WHERE p.[UserId] = @UserId
                    ORDER BY p.[CreatedAt] DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", userId }
                };

                var dataTable = await _db.ExecuteQueryAsync(sql, parameters);
                var posts = new List<Post>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var post = MapRowToPost(row);
                    
                    // Add community info if available
                    if (row.Table.Columns.Contains("CommunityName") && row["CommunityName"] != DBNull.Value)
                    {
                        post.Community = new Community
                        {
                            CommunityID = post.CommunityID ?? 0,
                            Name = row["CommunityName"].ToString(),
                            Slug = row["CommunitySlug"]?.ToString()
                        };
                    }
                    
                    posts.Add(post);
                }

                return posts;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving posts by user: {ex.Message}", ex);
            }
        }

        public Task<IEnumerable<Post>> GetFeaturedPostsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPromotedPostsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementViewCountAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            try
            {
                const string sql = @"
                    UPDATE Posts SET
                        Title = @Title,
                        Slug = @Slug,
                        Content = @Content,
                        Tags = @Tags,
                        FeaturedImage = @FeaturedImage,
                        PostStatus = @PostStatus,
                        AllowComments = @AllowComments,
                        IsPromoted = @IsPromoted,
                        IsFeatured = @IsFeatured,
                        UpdatedAt = GETUTCDATE(),
                        PublishedAt = CASE 
                            WHEN @PostStatus = 'Published' AND PublishedAt IS NULL 
                            THEN GETUTCDATE() 
                            ELSE PublishedAt 
                        END
                    WHERE PostID = @PostID AND UserId = @UserId;
                    
                    SELECT @@ROWCOUNT;";

                var parameters = new Dictionary<string, object>
                {
                    { "@PostID", post.PostID },
                    { "@UserId", post.UserId },
                    { "@Title", post.Title },
                    { "@Slug", post.Slug },
                    { "@Content", post.Content },
                    { "@Tags", (object)post.Tags ?? DBNull.Value },
                    { "@FeaturedImage", (object)post.FeaturedImage ?? DBNull.Value },
                    { "@PostStatus", post.PostStatus ?? "Draft" },
                    { "@AllowComments", post.AllowComments ?? true },
                    { "@IsPromoted", post.IsPromoted ?? false },
                    { "@IsFeatured", post.IsFeatured ?? false }
                };

                var rowsAffected = await _db.ExecuteScalarAsync<int>(sql, parameters);
                
                if (rowsAffected > 0)
                {
                    return post;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating post: {ex.Message}", ex);
            }
        }
    }
}
