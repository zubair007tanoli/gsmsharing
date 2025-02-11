using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface IChatRoomMemberRepository
    {
        Task<ChatRoomMember> GetByRoomAndUserIdAsync(int roomId, string userId);
        Task<IEnumerable<ChatRoomMember>> GetAllAsync();
        Task<IEnumerable<ChatRoomMember>> GetByRoomIdAsync(int roomId);
        Task<IEnumerable<ChatRoomMember>> GetByUserIdAsync(string userId);
        Task<ChatRoomMember> CreateAsync(ChatRoomMember chatRoomMember);
        Task<ChatRoomMember> UpdateAsync(ChatRoomMember chatRoomMember);
        Task DeleteAsync(int roomId, string userId);
        Task<int> GetMemberCountByRoomIdAsync(int roomId);
        Task<DateTime?> GetLastReadAtAsync(int roomId, string userId);
        Task UpdateLastReadAtAsync(int roomId, string userId, DateTime lastReadAt);
        Task<IEnumerable<ChatRoomMember>> GetRecentlyJoinedMembersAsync(int roomId, int count);
        Task<bool> IsMemberOfRoomAsync(int roomId, string userId);
    }
}
