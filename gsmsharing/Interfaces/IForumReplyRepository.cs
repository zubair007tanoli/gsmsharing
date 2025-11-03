using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface IForumReplyRepository
    {
        Task<IEnumerable<ForumReply>> GetRepliesByThreadIdAsync(int threadId);
        Task<ForumReply?> GetReplyByIdAsync(int replyId);
        Task<ForumReply> AddReplyAsync(ForumReply reply);
        Task<bool> UpdateReplyAsync(ForumReply reply);
        Task<bool> DeleteReplyAsync(int replyId);
    }
}

