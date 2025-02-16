using gsmsharing.Database;
using gsmsharing.ExeMethods;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.Models.SEO;

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

        public async Task<Post> CreateAsync(Post post)
        {
            try
            {
                String slugs = SlugGenerator.GenerateSlug(post.Slug);
                post.Slug = slugs;
                const string sql = @"
                    INSERT INTO Posts (
                        UserId, Title, Slug, MetaDescription, Content, FeaturedImage,
                        MetaTitle, PostStatus, AllowComments, CreatedAt, UpdatedAt, CommunityID
                    )
                    VALUES (
                        @UserId, @Title, @Slug, @MetaDescription, @Content, @FeaturedImage,
                        @MetaTitle,@PostStatus, @AllowComments, @CommunityID
                    );
                    SELECT SCOPE_IDENTITY();";

                var parameters = new Dictionary<string, object>
                {
                    { "@UserId", post.UserId },
                    { "@Title", post.Title },
                    { "@Slug", post.Slug },
                    { "@MetaDescription", (object)post.MetaDescription ?? DBNull.Value },
                    { "@Content", post.Content },
                    { "@FeaturedImage", (object)post.FeaturedImage ?? DBNull.Value },             
                    { "@PostStatus", post.PostStatus ?? "draft" },
                    { "@AllowComments", post.AllowComments ?? true },
                    { "@CommunityID", (object)post.CommunityID ?? DBNull.Value }
                };

                return await _db.ExecuteScalarAsync<Post>(sql, parameters);
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

        public Task<IEnumerable<Post>> GetAllAsync()
        {
            throw new NotImplementedException();
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
