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

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
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
                (SELECT COUNT(*) FROM Reactions r WHERE r.PostID = p.PostID) AS TotalReactions,
                (SELECT COUNT(*) FROM Reactions r WHERE r.PostID = p.PostID AND r.ReactionType = 'Like') AS LikeCount,
                (SELECT COUNT(*) FROM Reactions r WHERE r.PostID = p.PostID AND r.ReactionType = 'Love') AS LoveCount
            FROM Posts p
            LEFT JOIN PostSEO s ON p.[PostID] = s.[PostID]
            LEFT JOIN AspNetUsers u ON p.[UserId] = u.[Id]
            LEFT JOIN Communities c ON p.[CommunityID] = c.[CommunityID]
            ORDER BY p.[CreatedAt] DESC";

                var dataTable = await _db.ExecuteQueryAsync(sql);

                var posts = new List<PostViewModelDisplay>();

                foreach (DataRow row in dataTable.Rows)
                {
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
                            TotalReactions = Convert.ToInt32(row["TotalReactions"]),
                            LikeCount = Convert.ToInt32(row["LikeCount"]),
                            LoveCount = Convert.ToInt32(row["LoveCount"])
                        }
                    };

                    posts.Add(post);
                }

                return posts;
            }
            catch (Exception ex)
            {
            
                throw;
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

        public Task<Post> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetBySlugAsync(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
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

        public Task<Post> UpdateAsync(Post post)
        {
            throw new NotImplementedException();
        }
    }
}
