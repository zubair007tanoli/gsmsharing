using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface IForumThreadRepository
    {
        Task<IEnumerable<ForumThread>> GetAllThreadsAsync();
        Task<ForumThread?> GetThreadByIdAsync(int threadId);
        Task<IEnumerable<ForumThread>> GetPublishedThreadsAsync();
        Task<ForumThread> AddThreadAsync(ForumThread thread);
        Task<bool> UpdateThreadAsync(ForumThread thread);
        Task<bool> DeleteThreadAsync(int threadId);
        Task IncrementViewCountAsync(int threadId);
    }
}

