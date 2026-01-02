using GsmsharingV2.Models.NewSchema;

namespace GsmsharingV2.Interfaces
{
    /// <summary>
    /// Repository interface for Reddit-style forum posts
    /// </summary>
    public interface IForumPostRepository
    {
        Task<ForumPost?> GetByIdAsync(long id);
        Task<ForumPost?> GetBySlugAsync(string slug);
        Task<IEnumerable<ForumPost>> GetAllAsync();
        Task<IEnumerable<ForumPost>> GetByCommunityIdAsync(long communityId);
        Task<IEnumerable<ForumPost>> GetByPostTypeAsync(string postType);
        Task<IEnumerable<ForumPost>> GetByUserIdAsync(long userId);
        
        // Reddit-style sorting
        Task<IEnumerable<ForumPost>> GetHotPostsAsync(int count = 20);
        Task<IEnumerable<ForumPost>> GetTopPostsAsync(int count = 20, string timeframe = "all"); // all, day, week, month, year
        Task<IEnumerable<ForumPost>> GetNewPostsAsync(int count = 20);
        Task<IEnumerable<ForumPost>> GetRisingPostsAsync(int count = 20);
        
        // Pagination
        Task<IEnumerable<ForumPost>> GetPaginatedAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        
        // CRUD operations
        Task<ForumPost> CreateAsync(ForumPost post);
        Task<ForumPost> UpdateAsync(ForumPost post);
        Task DeleteAsync(long id);
        
        // Engagement
        Task IncrementViewCountAsync(long id);
        Task VoteAsync(long postId, long userId, string voteType); // upvote, downvote
        Task RemoveVoteAsync(long postId, long userId);
        Task UpdateScoreAsync(long postId);
    }
}








