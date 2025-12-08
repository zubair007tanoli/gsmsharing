using GsmsharingV2.DTOs;

namespace GsmsharingV2.Interfaces
{
    public interface IPostService
    {
        Task<PostDto?> GetByIdAsync(int id);
        Task<PostDto?> GetBySlugAsync(string slug);
        Task<PostDto?> GetBySlugAndCommunityAsync(string slug, string communitySlug);
        Task<IEnumerable<PostDto>> GetAllAsync();
        Task<IEnumerable<PostDto>> GetByCommunityIdAsync(int communityId);
        Task<IEnumerable<PostDto>> GetByUserIdAsync(string userId);
        Task<PostDto> CreateAsync(CreatePostDto createPostDto, string userId);
        Task<PostDto> UpdateAsync(UpdatePostDto updatePostDto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<PostDto>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<PostDto>> GetFeaturedPostsAsync();
        Task<IEnumerable<PostDto>> GetRecentPostsAsync(int count);
        Task IncrementViewCountAsync(int id);
    }
}

