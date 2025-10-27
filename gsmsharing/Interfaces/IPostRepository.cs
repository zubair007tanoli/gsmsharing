using gsmsharing.Models;
using gsmsharing.ViewModels;

namespace gsmsharing.Interfaces
{
    public interface IPostRepository
    {
        Task<PostViewModelWithSEO> GetByIdAsync(int id);
        Task<Post> GetBySlugAsync(string slug);
        Task<PostViewModelWithSEO> GetBySlugAndCommunityAsync(string slug, string communitySlug);
        Task<IEnumerable<PostViewModelDisplay>> GetAllAsync();
        Task<IEnumerable<Post>> GetByCommunityIdAsync(int communityId);
        Task<IEnumerable<Post>> GetByUserIdAsync(string userId);
        Task<int> CreateAsync(Post post);    
        Task<Post> UpdateAsync(Post post);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Post>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<Post>> GetFeaturedPostsAsync();
        Task<IEnumerable<Post>> GetPromotedPostsAsync();
        Task<IEnumerable<Post>> GetByStatusAsync(string status);
        Task<IEnumerable<Post>> GetRecentPostsAsync(int count);
        Task<int> IncrementViewCountAsync(int id);
    }
}
