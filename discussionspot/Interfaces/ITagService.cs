using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<string>> GetPopularTagsAsync(int count);
        Task<IEnumerable<PostViewModel>> GetPostsByTagAsync(string tagSlug, int page, int pageSize);
        Task<IEnumerable<string>> SearchTagsAsync(string searchTerm);
    }
}
