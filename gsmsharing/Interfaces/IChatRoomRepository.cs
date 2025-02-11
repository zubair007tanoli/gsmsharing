using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom> GetByIdAsync(int id);
        Task<IEnumerable<ChatRoom>> GetAllAsync();
        Task<IEnumerable<ChatRoom>> GetByCommunityIdAsync(int communityId);
        Task<IEnumerable<ChatRoom>> GetByRoomTypeAsync(string roomType);
        Task<ChatRoom> CreateAsync(ChatRoom chatRoom);
        Task<ChatRoom> UpdateAsync(ChatRoom chatRoom);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<ChatRoom>> GetPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<ChatRoom>> GetByCreatedByAsync(string userId);
        Task<IEnumerable<ChatRoom>> GetRecentlyCreatedRoomsAsync(int count);
        Task<bool> RoomExistsAsync(int roomId);
    }
}
