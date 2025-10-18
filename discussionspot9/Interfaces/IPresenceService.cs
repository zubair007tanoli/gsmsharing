using discussionspot9.Models.Domain;

namespace discussionspot9.Interfaces
{
    public interface IPresenceService
    {
        Task UserConnectedAsync(string userId, string connectionId);
        Task<bool> UserDisconnectedAsync(string userId, string connectionId);
        Task UpdateUserStatusAsync(string userId, string status);
        Task UpdateCurrentPageAsync(string userId, string currentPage);
        Task UpdateTypingStatusAsync(string userId, string typingWithUserId, bool isTyping);
        Task<UserPresence?> GetUserPresenceAsync(string userId);
        Task<List<string>> GetOnlineUserIdsAsync();
        Task<Dictionary<string, string>> GetUserStatusesAsync(List<string> userIds);
        Task<bool> IsUserOnlineAsync(string userId);
    }
}

