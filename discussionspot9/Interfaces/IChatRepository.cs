using discussionspot9.Models.Domain;

namespace discussionspot9.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetDirectMessagesAsync(string userId1, string userId2, int skip, int take);
        Task<List<ChatMessage>> GetRoomMessagesAsync(int roomId, int skip, int take);
        Task<ChatMessage?> GetMessageByIdAsync(int messageId);
        Task UpdateMessageAsync(ChatMessage message);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<ChatMessage>> GetUnreadMessagesAsync(string userId);
        
        Task<ChatRoom> CreateRoomAsync(ChatRoom room);
        Task<ChatRoom?> GetRoomByIdAsync(int roomId);
        Task<List<ChatRoom>> GetUserRoomsAsync(string userId);
        Task<ChatRoomMember> AddRoomMemberAsync(ChatRoomMember member);
        Task RemoveRoomMemberAsync(int roomId, string userId);
        Task<bool> IsUserInRoomAsync(int roomId, string userId);
    }
}

