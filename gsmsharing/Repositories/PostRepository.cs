using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.Models.SEO;

namespace gsmsharing.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly PostSEODataAccess _postSEODataAccess;

        public PostRepository(PostSEODataAccess postSEODataAccess)
        {
            _postSEODataAccess = postSEODataAccess;
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

          var chek = await CreateArticleSchema(1, "title", "authorName");
            return null;
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
