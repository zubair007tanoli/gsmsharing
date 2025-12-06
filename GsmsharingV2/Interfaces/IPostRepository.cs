using GsmsharingV2.Models;

namespace GsmsharingV2.Interfaces
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(int id);
        Task<Post?> GetBySlugAsync(string slug);
        Task<Post?> GetBySlugAndCommunityAsync(string slug, string communitySlug);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<IEnumerable<Post>> GetByCommunityIdAsync(int communityId);
        Task<IEnumerable<Post>> GetByUserIdAsync(string userId);
        Task<Post> CreateAsync(Post post);
        Task<Post> UpdateAsync(Post post);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Post>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<Post>> GetFeaturedPostsAsync();
        Task<IEnumerable<Post>> GetRecentPostsAsync(int count);
        Task IncrementViewCountAsync(int id);
    }
}

