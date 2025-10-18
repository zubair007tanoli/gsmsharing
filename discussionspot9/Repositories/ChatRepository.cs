using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            message.SentAt = DateTime.UtcNow;
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetDirectMessagesAsync(string userId1, string userId2, int skip, int take)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                           (m.SenderId == userId2 && m.ReceiverId == userId1))
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetRoomMessagesAsync(int roomId, int skip, int take)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .Include(m => m.Sender)
                .ToListAsync();
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<List<ChatMessage>> GetUnreadMessagesAsync(string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatRoom> CreateRoomAsync(ChatRoom room)
        {
            room.CreatedAt = DateTime.UtcNow;
            room.IsActive = true;
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<ChatRoom?> GetRoomByIdAsync(int roomId)
        {
            return await _context.ChatRooms
                .Include(r => r.Members)
                .FirstOrDefaultAsync(r => r.ChatRoomId == roomId);
        }

        public async Task<List<ChatRoom>> GetUserRoomsAsync(string userId)
        {
            return await _context.ChatRoomMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.ChatRoom)
                .Select(m => m.ChatRoom)
                .ToListAsync();
        }

        public async Task<ChatRoomMember> AddRoomMemberAsync(ChatRoomMember member)
        {
            member.JoinedAt = DateTime.UtcNow;
            _context.ChatRoomMembers.Add(member);
            
            // Update room member count
            var room = await _context.ChatRooms.FindAsync(member.ChatRoomId);
            if (room != null)
            {
                room.MemberCount++;
                await _context.SaveChangesAsync();
            }
            
            return member;
        }

        public async Task RemoveRoomMemberAsync(int roomId, string userId)
        {
            var member = await _context.ChatRoomMembers
                .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
            
            if (member != null)
            {
                _context.ChatRoomMembers.Remove(member);
                
                // Update room member count
                var room = await _context.ChatRooms.FindAsync(roomId);
                if (room != null && room.MemberCount > 0)
                {
                    room.MemberCount--;
                }
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserInRoomAsync(int roomId, string userId)
        {
            return await _context.ChatRoomMembers
                .AnyAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
        }
    }
}

