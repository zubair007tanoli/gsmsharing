using gsmsharing.Database;
using Newtonsoft.Json;
using System.Text.Json;

namespace gsmsharing.Models.SEO
{
    public class PostSEODataAccess
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<PostSEODataAccess> _logger;

        public PostSEODataAccess(DatabaseConnection db, ILogger<PostSEODataAccess> logger)
        {
            _db = db;
            _logger = logger;
        }

        public class SchemaData
        {
            public string Context { get; set; } = "https://schema.org";
            public string Type { get; set; }
            public Dictionary<string, object> Properties { get; set; }
        }

        public enum SchemaType
        {
            Article,
            BlogPosting,
            NewsArticle,
            Product,
            FAQPage,
            HowTo
        }

        public async Task<int> InsertSchemaData(int postId, SchemaType schemaType, Dictionary<string, object> schemaProperties)
        {
            try
            {
                var schemaData = new SchemaData
                {
                    Type = schemaType.ToString(),
                    Properties = ValidateSchemaProperties(schemaType, schemaProperties)
                };

                string jsonSchema = System.Text.Json.JsonSerializer.Serialize(
                    new Dictionary<string, object>
                    {
                    { "@context", schemaData.Context },
                    { "@type", schemaData.Type }
                    }.Concat(schemaProperties)
                    .ToDictionary(k => k.Key, v => v.Value),
                    new JsonSerializerOptions { WriteIndented = true }
                );

                const string sql = @"
                INSERT INTO PostSEO (
                    PostID, 
                    Schema, 
                    CreatedAt,
                    UpdatedAt
                )
                VALUES (
                    @PostID,
                    @Schema,
                    GETDATE(),
                    GETDATE()
                );
                SELECT SCOPE_IDENTITY();";

                var parameters = new Dictionary<string, object>
            {
                { "@PostID", postId },
                { "@Schema", jsonSchema }
            };

                return await _db.ExecuteScalarAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting schema for post {PostId}", postId);
                throw;
            }
        }

        private Dictionary<string, object> ValidateSchemaProperties(SchemaType schemaType, Dictionary<string, object> properties)
        {
            var requiredProperties = GetRequiredProperties(schemaType);
            var missingProperties = requiredProperties.Except(properties.Keys).ToList();

            if (missingProperties.Any())
            {
                throw new ArgumentException(
                    $"Missing required properties for {schemaType}: {string.Join(", ", missingProperties)}");
            }

            return properties;
        }

        private IEnumerable<string> GetRequiredProperties(SchemaType schemaType)
        {
            return schemaType switch
            {
                SchemaType.Article or SchemaType.BlogPosting or SchemaType.NewsArticle =>
                    new[] { "headline", "author", "datePublished", "dateModified" },

                SchemaType.Product =>
                    new[] { "name", "description", "offers" },

                SchemaType.FAQPage =>
                    new[] { "mainEntity" },

                SchemaType.HowTo =>
                    new[] { "name", "description", "step" },

                _ => throw new ArgumentException($"Unsupported schema type: {schemaType}")
            };
        }

        public async Task<bool> UpdatePostSchema(int postId, SchemaData schemaData)
        {
            try
            {
                if (string.IsNullOrEmpty(schemaData.Type))
                {
                    throw new ArgumentException("Schema type is required");
                }

                var jsonObject = new Dictionary<string, object>
            {
                { "@context", schemaData.Context },
                { "@type", schemaData.Type }
            };

                foreach (var property in schemaData.Properties)
                {
                    jsonObject.Add(property.Key, property.Value);
                }

                string jsonSchema = System.Text.Json.JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                const string sql = @"
                IF EXISTS (SELECT 1 FROM PostSEO WHERE PostID = @PostID)
                    UPDATE PostSEO 
                    SET Schema = @Schema,
                        UpdatedAt = GETDATE()
                    WHERE PostID = @PostID
                ELSE
                    INSERT INTO PostSEO (PostID, Schema, CreatedAt)
                    VALUES (@PostID, @Schema, GETDATE());";

                var parameters = new Dictionary<string, object>
            {
                { "@PostID", postId },
                { "@Schema", jsonSchema }
            };

                int result = await _db.ExecuteNonQueryAsync(sql, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schema for post {PostId}", postId);
                throw;
            }
        }

        public async Task<string> GetPostSchema(int postId)
        {
            const string sql = "SELECT Schema FROM PostSEO WHERE PostID = @PostID";
            var parameters = new Dictionary<string, object>
        {
            { "@PostID", postId }
        };

            return await _db.ExecuteScalarAsync<string>(sql, parameters);
        }
    }
}
