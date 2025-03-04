using gsmsharing.Database;
using gsmsharing.ExeMethods;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace gsmsharing.Repositories
{
    public class SEORepository : ISeo
    {
        private readonly DatabaseConnection _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _websiteUrl;

        public SEORepository(
            DatabaseConnection dbConnection,
            UserManager<ApplicationUser> userManager,
            string websiteUrl = "https://gsmsharing.com")
        {
            _db = dbConnection;
            _userManager = userManager;
            _websiteUrl = websiteUrl;
        }

        public async Task<bool> GenerateAndSaveSchema(Post post, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception($"User with ID {userId} not found");
                }

                var schemaData = await GenerateSchemaFromPost(post, user);
                return await SaveSchema(post.PostID, post, schemaData);
            }
            catch (Exception ex)
            {
                // Log the error if you have a logger
                throw new Exception("Error generating schema", ex);
            }
        }

        public async Task<bool> SaveCustomSchema(int postId, string schemaType, Dictionary<string, object> properties)
        {
            try
            {
                var schemaData = new Dictionary<string, object>
                {
                    { "@context", "https://schema.org" },
                    { "@type", schemaType }
                };

                // Add all custom properties to the schema
                foreach (var property in properties)
                {
                    schemaData.Add(property.Key, property.Value);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving custom schema for post {postId}", ex);
            }
        }

        public async Task<Dictionary<string, object>> GetSchemaForPost(int postId)
        {
            try
            {
                const string sql = "SELECT Schema FROM PostSEO WHERE PostID = @PostID";
                var parameters = new Dictionary<string, object>
                {
                    { "@PostID", postId }
                };

                string jsonSchema = await _db.ExecuteScalarAsync<string>(sql, parameters);

                if (string.IsNullOrEmpty(jsonSchema))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<Dictionary<string, object>>(
                    jsonSchema,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving schema for post {postId}", ex);
            }
        }

        public async Task<bool> DeleteSchema(int postId)
        {
            try
            {
                const string sql = "DELETE FROM PostSEO WHERE PostID = @PostID";
                var parameters = new Dictionary<string, object>
                {
                    { "@PostID", postId }
                };

                int result = await _db.ExecuteNonQueryAsync(sql, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting schema for post {postId}", ex);
            }
        }

        private async Task<Dictionary<string, object>> GenerateSchemaFromPost(Post post, IdentityUser user)
        {
            var postUrl = $"{_websiteUrl}/posts/{post.Slug}";
            
            var schema = new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@type", "BlogPosting" },
                { "headline", post.Title },
                { "description", post.MetaDescription ?? post.MetaDescription ?? "" },
                { "author", new Dictionary<string, object>
                    {
                        { "@type", "Person" },
                        { "name", user.UserName },
                        { "@id", $"{_websiteUrl}/authors/{user.Id}" }
                    }
                },
                { "datePublished", post.PublishedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "dateModified", post.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "mainEntityOfPage", new Dictionary<string, object>
                    {
                        { "@type", "WebPage" },
                        { "@id", postUrl }
                    }
                }
            };

            // Add image if available
            if (!string.IsNullOrEmpty(post.FeaturedImage))
            {
                schema.Add("image", new Dictionary<string, object>
                {
                    { "@type", "ImageObject" },
                    { "url", GetFullImageUrl(post.FeaturedImage) },
                    { "height", "800" },
                    { "width", "1200" }
                });
            }

            return schema;
        }
        private async Task<bool> SaveSchema(int postId, Post post, Dictionary<string, object> schemaData)
        {
            string jsonSchema = JsonSerializer.Serialize(schemaData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            const string sql = @"
    IF EXISTS (SELECT 1 FROM PostSEO WHERE PostID = @PostID)
    BEGIN
        UPDATE PostSEO 
        SET [Schema] = @Schema,
            MetaTitle = @MetaTitle,
            MetaDescription = @MetaDescription,
            MetaKeywords = @MetaKeywords,
            OgTitle = @OgTitle,
            OgDescription = @OgDescription,
            OgImage = @OgImage,
            TwitterCard = @TwitterCard,
            TwitterTitle = @TwitterTitle,
            TwitterDescription = @TwitterDescription,
            TwitterImage = @TwitterImage,
            CanonicalURL = @CanonicalURL,
            Robots = @Robots
        WHERE PostID = @PostID;

        SELECT SEOId FROM PostSEO WHERE PostID = @PostID;
    END
    ELSE
    BEGIN
        INSERT INTO PostSEO (
            PostID,
            MetaTitle,
            MetaDescription,
            MetaKeywords,
            OgTitle,
            OgDescription,
            OgImage,
            TwitterCard,
            TwitterTitle,
            TwitterDescription,
            TwitterImage,
            CanonicalURL,
            Robots,
            [Schema]
        )
        OUTPUT INSERTED.SEOId
        VALUES (
            @PostID,
            @MetaTitle,
            @MetaDescription,
            @MetaKeywords,
            @OgTitle,
            @OgDescription,
            @OgImage,
            @TwitterCard,
            @TwitterTitle,
            @TwitterDescription,
            @TwitterImage,
            @CanonicalURL,
            @Robots,
            @Schema
        );
    END;";
            post.OgTitle = SlugGenerator.ShortenTitle(post.Title);
            post.OgDescription = SlugGenerator.ShortenDescription(post.MetaDescription);
            post.OgImage = post.FeaturedImage;
            


            // Assuming the Post model might not have all these properties,
            // you may need to adjust based on your actual Post model structure
            var parameters = new Dictionary<string, object>
                {
                    { "@PostID", postId },
                    { "@Schema", jsonSchema },
                    { "@MetaTitle", post.MetaTitle ?? (object)DBNull.Value },
                    { "@MetaDescription", post.MetaDescription ?? (object)DBNull.Value },
                    { "@MetaKeywords", post.Tags ?? (object)DBNull.Value },
                    { "@OgTitle", post.OgTitle ?? (object)DBNull.Value },
                    { "@OgDescription", post.OgDescription ?? (object)DBNull.Value },
                    { "@OgImage", post.FeaturedImage ?? (object)DBNull.Value },
                    { "@TwitterCard", "summary" },
                    { "@TwitterTitle", post.OgTitle ?? (Object)DBNull.Value },
                    { "@TwitterDescription", post.OgDescription ?? (object) DBNull.Value  },
                    { "@TwitterImage",  post.FeaturedImage ?? (object)DBNull.Value },
                    { "@CanonicalURL", GetPropertyValueOrDefault(post, "CanonicalURL") },
                    { "@Robots", "index, follow" }
                };

            var result = await _db.ExecuteNonQueryAsync(sql, parameters);
            return result > 0;
        }

        private object GetPropertyValueOrDefault(Post post, string propertyName)
        {
            var property = post.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(post);
                return value ?? (object)DBNull.Value;
            }
            return DBNull.Value;
        }
        //private async Task<bool> SaveSchema(int postId, Dictionary<string, object> schemaData)
        //{
        //    string jsonSchema = JsonSerializer.Serialize(schemaData, new JsonSerializerOptions
        //    {
        //        WriteIndented = true
        //    });

        //    const string sql = @"
        //        IF EXISTS (SELECT 1 FROM PostSEO WHERE PostID = @PostID)
        //            UPDATE PostSEO 
        //            SET Schema = @Schema,
        //                UpdatedAt = GETDATE()
        //            WHERE PostID = @PostID
        //        ELSE
        //            INSERT INTO PostSEO (
        //                PostID, 
        //                Schema, 
        //                CreatedAt,
        //                UpdatedAt
        //            )
        //            VALUES (
        //                @PostID,
        //                @Schema,
        //                GETDATE(),
        //                GETDATE()
        //            )";

        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "@PostID", postId },
        //        { "@Schema", jsonSchema }
        //    };

        //    var result = await _db.ExecuteNonQueryAsync(sql, parameters);
        //    return result > 0;
        //}

        private string GetFullImageUrl(string imageUrl)
        {
            if (imageUrl.StartsWith("http"))
                return imageUrl;

            return $"{_websiteUrl}{imageUrl}";
        }
    }
}
