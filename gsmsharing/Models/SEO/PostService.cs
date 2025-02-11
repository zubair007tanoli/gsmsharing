namespace gsmsharing.Models.SEO
{
    public class PostService
    {
        private readonly PostSEODataAccess _postSEODataAccess;

        public PostService(PostSEODataAccess postSEODataAccess)
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
    }
}
