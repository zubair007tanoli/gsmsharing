using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface ISeo
    {
        Task<bool> GenerateAndSaveSchema(Post post, string userId);
        Task<bool> SaveCustomSchema(int postId, string schemaType, Dictionary<string, object> properties);
        Task<Dictionary<string, object>> GetSchemaForPost(int postId);
        Task<bool> DeleteSchema(int postId);
    }
}
