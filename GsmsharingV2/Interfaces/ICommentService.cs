using GsmsharingV2.DTOs;

namespace GsmsharingV2.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto?> GetByIdAsync(int id);
        Task<IEnumerable<CommentDto>> GetByPostIdAsync(int postId);
        Task<IEnumerable<CommentDto>> GetByUserIdAsync(string userId);
        Task<IEnumerable<CommentDto>> GetRepliesAsync(int parentCommentId);
        Task<CommentDto> CreateAsync(CreateCommentDto createCommentDto, string userId);
        Task<CommentDto> UpdateAsync(UpdateCommentDto updateCommentDto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<int> GetCommentCountAsync(int postId);
    }
}

