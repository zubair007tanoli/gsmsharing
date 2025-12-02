using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace discussionspot9.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ChatRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            try
            {
                // Ensure required fields are set
                if (string.IsNullOrEmpty(message.SenderId))
                {
                    throw new ArgumentException("SenderId cannot be null or empty");
                }
                
                if (string.IsNullOrWhiteSpace(message.Content))
                {
                    throw new ArgumentException("Content cannot be null or empty");
                }
                
                // Set sent time if not already set
                if (message.SentAt == default)
                {
                    message.SentAt = DateTime.UtcNow;
                }
                
                // Add and save message (optimized - removed unnecessary checks)
                context.ChatMessages.Add(message);
                
                // Save changes - MessageId is automatically set by EF Core
                await context.SaveChangesAsync();
                
                // Return the message directly - EF Core tracks it and MessageId is set
                return message;
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException($"[ChatRepository] Database error saving message: {dbEx.Message}", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"[ChatRepository] Failed to save message: {ex.Message}", ex);
            }
        }

        public async Task<List<ChatMessage>> GetDirectMessagesAsync(string userId1, string userId2, int skip, int take)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Optimized query - removed unnecessary Includes for better performance
            // Use AsNoTracking for read-only queries
            return await context.ChatMessages
                .AsNoTracking()
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                           (m.SenderId == userId2 && m.ReceiverId == userId1))
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetRoomMessagesAsync(int roomId, int skip, int take)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Optimized query - removed unnecessary Include for better performance
            return await context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // Use AsNoTracking for read-only queries
            return await context.ChatMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.ChatMessages.Update(message);
            await context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<List<ChatMessage>> GetUnreadMessagesAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatRoom> CreateRoomAsync(ChatRoom room)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            room.CreatedAt = DateTime.UtcNow;
            room.IsActive = true;
            context.ChatRooms.Add(room);
            await context.SaveChangesAsync();
            return room;
        }

        public async Task<ChatRoom?> GetRoomByIdAsync(int roomId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatRooms
                .Include(r => r.Members)
                .FirstOrDefaultAsync(r => r.ChatRoomId == roomId);
        }

        public async Task<List<ChatRoom>> GetUserRoomsAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatRoomMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.ChatRoom)
                .Select(m => m.ChatRoom)
                .ToListAsync();
        }

        public async Task<ChatRoomMember> AddRoomMemberAsync(ChatRoomMember member)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            member.JoinedAt = DateTime.UtcNow;
            context.ChatRoomMembers.Add(member);
            
            // Update room member count
            var room = await context.ChatRooms.FindAsync(member.ChatRoomId);
            if (room != null)
            {
                room.MemberCount++;
                await context.SaveChangesAsync();
            }
            
            return member;
        }

        public async Task RemoveRoomMemberAsync(int roomId, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var member = await context.ChatRoomMembers
                .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
            
            if (member != null)
            {
                context.ChatRoomMembers.Remove(member);
                
                // Update room member count
                var room = await context.ChatRooms.FindAsync(roomId);
                if (room != null && room.MemberCount > 0)
                {
                    room.MemberCount--;
                }
                
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserInRoomAsync(int roomId, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatRoomMembers
                .AnyAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
        }
    }
}

