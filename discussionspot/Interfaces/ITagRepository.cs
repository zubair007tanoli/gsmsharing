using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetTagBySlugAsync(string slug);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
        Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm);
        Task<Tag?> GetOrCreateTagAsync(string name);
        Task UpdatePostCountAsync(int tagId);
        Task<bool> SlugExistsAsync(string slug);
    }
}
