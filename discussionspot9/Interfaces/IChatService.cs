using discussionspot9.Models.ViewModels.ChatViewModels;

namespace discussionspot9.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessageViewModel> SendDirectMessageAsync(string senderId, string receiverId, string content, string? attachmentUrl = null);
        Task<ChatMessageViewModel> SendRoomMessageAsync(string senderId, int roomId, string content, string? attachmentUrl = null);
        Task<List<ChatMessageViewModel>> GetDirectChatHistoryAsync(string userId, string otherUserId, int page = 1, int pageSize = 50);
        Task<List<ChatMessageViewModel>> GetRoomChatHistoryAsync(int roomId, int page = 1, int pageSize = 50);
        Task<List<DirectChatViewModel>> GetUserDirectChatsAsync(string userId);
        Task<List<ChatRoomViewModel>> GetUserChatRoomsAsync(string userId);
        Task MarkMessageAsReadAsync(int messageId, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<ChatRoomViewModel> CreateChatRoomAsync(string creatorId, string name, string? description, bool isPublic = true);
        Task JoinChatRoomAsync(int roomId, string userId);
        Task LeaveChatRoomAsync(int roomId, string userId);
        Task<bool> IsUserInRoomAsync(int roomId, string userId);
    }
}

