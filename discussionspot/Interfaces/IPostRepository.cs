using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<Post?> GetPostBySlugAsync(string communitySlug, string postSlug);
        Task<Post?> GetPostWithDetailsAsync(int postId);
        Task<IEnumerable<Post>> GetPostsByCommunityAsync(int communityId, int page, int pageSize);
        Task<IEnumerable<Post>> GetPostsByUserAsync(string userId, int page, int pageSize);
        Task<IEnumerable<Post>> GetPostsByCategoryAsync(int categoryId, int page, int pageSize);
        Task<IEnumerable<Post>> GetPostsByTagAsync(int tagId, int page, int pageSize);
        Task<IEnumerable<Post>> GetHotPostsAsync(int page, int pageSize);
        Task<IEnumerable<Post>> GetNewPostsAsync(int page, int pageSize);
        Task<IEnumerable<Post>> GetTopPostsAsync(string timeFilter, int page, int pageSize);
        Task<IEnumerable<Post>> GetTrendingPostsAsync(int count);
        Task<IEnumerable<Post>> SearchPostsAsync(string searchTerm, int page, int pageSize);
        Task UpdatePostScoreAsync(int postId);
        Task UpdateCommentCountAsync(int postId);
        Task UpdateVoteCountsAsync(int postId);
        Task IncrementViewCountAsync(int postId);
        Task<bool> SlugExistsInCommunityAsync(int communityId, string slug);
    }
}
