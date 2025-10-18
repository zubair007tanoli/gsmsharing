using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.ChatViewModels;
using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepository,
            ApplicationDbContext context,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<ChatMessageViewModel> SendDirectMessageAsync(string senderId, string receiverId, string content, string? attachmentUrl = null)
        {
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                AttachmentUrl = attachmentUrl,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                IsDeleted = false
            };

            var savedMessage = await _chatRepository.AddMessageAsync(message);
            
            // Get sender info
            var sender = await _context.UserProfiles.FindAsync(senderId);
            
            return MapToViewModel(savedMessage, senderId, sender?.DisplayName ?? "Unknown");
        }

        public async Task<ChatMessageViewModel> SendRoomMessageAsync(string senderId, int roomId, string content, string? attachmentUrl = null)
        {
            // Check if user is in room
            var isInRoom = await _chatRepository.IsUserInRoomAsync(roomId, senderId);
            if (!isInRoom)
            {
                throw new UnauthorizedAccessException("User is not a member of this room");
            }

            var message = new ChatMessage
            {
                SenderId = senderId,
                ChatRoomId = roomId,
                Content = content,
                AttachmentUrl = attachmentUrl,
                SentAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var savedMessage = await _chatRepository.AddMessageAsync(message);
            
            // Get sender info
            var sender = await _context.UserProfiles.FindAsync(senderId);
            
            return MapToViewModel(savedMessage, senderId, sender?.DisplayName ?? "Unknown");
        }

        public async Task<List<ChatMessageViewModel>> GetDirectChatHistoryAsync(string userId, string otherUserId, int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            var messages = await _chatRepository.GetDirectMessagesAsync(userId, otherUserId, skip, pageSize);
            
            return messages.Select(m => MapToViewModel(m, userId, m.Sender?.UserName ?? "Unknown")).Reverse().ToList();
        }

        public async Task<List<ChatMessageViewModel>> GetRoomChatHistoryAsync(int roomId, int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            var messages = await _chatRepository.GetRoomMessagesAsync(roomId, skip, pageSize);
            
            return messages.Select(m => MapToViewModel(m, m.SenderId, m.Sender?.UserName ?? "Unknown")).Reverse().ToList();
        }

        public async Task<List<DirectChatViewModel>> GetUserDirectChatsAsync(string userId)
        {
            // Get all messages where user is sender or receiver
            var recentChats = await _context.ChatMessages
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted && m.ChatRoomId == null)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault(),
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                })
                .ToListAsync();

            var chats = new List<DirectChatViewModel>();
            foreach (var chat in recentChats)
            {
                if (string.IsNullOrEmpty(chat.OtherUserId)) continue;
                
                var otherUser = await _context.UserProfiles.FindAsync(chat.OtherUserId);
                if (otherUser == null) continue;

                chats.Add(new DirectChatViewModel
                {
                    UserId = chat.OtherUserId,
                    DisplayName = otherUser.DisplayName,
                    AvatarUrl = otherUser.AvatarUrl ?? "/images/default-avatar.png",
                    Status = "online", // Will be updated by PresenceService
                    UnreadCount = chat.UnreadCount,
                    LastMessage = chat.LastMessage != null ? MapToViewModel(chat.LastMessage, userId, chat.LastMessage.Sender?.UserName ?? "Unknown") : null
                });
            }

            return chats.OrderByDescending(c => c.LastMessage?.SentAt ?? DateTime.MinValue).ToList();
        }

        public async Task<List<ChatRoomViewModel>> GetUserChatRoomsAsync(string userId)
        {
            var rooms = await _chatRepository.GetUserRoomsAsync(userId);
            
            return rooms.Select(r => new ChatRoomViewModel
            {
                ChatRoomId = r.ChatRoomId,
                Name = r.Name,
                Description = r.Description,
                IconUrl = r.IconUrl,
                MemberCount = r.MemberCount,
                IsPublic = r.IsPublic,
                UnreadCount = 0 // TODO: Calculate unread count
            }).ToList();
        }

        public async Task MarkMessageAsReadAsync(int messageId, string userId)
        {
            var message = await _chatRepository.GetMessageByIdAsync(messageId);
            if (message != null && message.ReceiverId == userId && !message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _chatRepository.UpdateMessageAsync(message);
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _chatRepository.GetUnreadCountAsync(userId);
        }

        public async Task<ChatRoomViewModel> CreateChatRoomAsync(string creatorId, string name, string? description, bool isPublic = true)
        {
            var room = new ChatRoom
            {
                Name = name,
                Description = description,
                CreatorId = creatorId,
                IsPublic = isPublic,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                MemberCount = 1
            };

            var savedRoom = await _chatRepository.CreateRoomAsync(room);

            // Add creator as member
            await _chatRepository.AddRoomMemberAsync(new ChatRoomMember
            {
                ChatRoomId = savedRoom.ChatRoomId,
                UserId = creatorId,
                Role = "admin",
                JoinedAt = DateTime.UtcNow
            });

            return new ChatRoomViewModel
            {
                ChatRoomId = savedRoom.ChatRoomId,
                Name = savedRoom.Name,
                Description = savedRoom.Description,
                IconUrl = savedRoom.IconUrl,
                MemberCount = savedRoom.MemberCount,
                IsPublic = savedRoom.IsPublic
            };
        }

        public async Task JoinChatRoomAsync(int roomId, string userId)
        {
            var isAlreadyMember = await _chatRepository.IsUserInRoomAsync(roomId, userId);
            if (isAlreadyMember)
            {
                throw new InvalidOperationException("User is already a member of this room");
            }

            await _chatRepository.AddRoomMemberAsync(new ChatRoomMember
            {
                ChatRoomId = roomId,
                UserId = userId,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            });
        }

        public async Task LeaveChatRoomAsync(int roomId, string userId)
        {
            await _chatRepository.RemoveRoomMemberAsync(roomId, userId);
        }

        private ChatMessageViewModel MapToViewModel(ChatMessage message, string currentUserId, string senderName)
        {
            return new ChatMessageViewModel
            {
                MessageId = message.MessageId,
                SenderId = message.SenderId,
                SenderName = senderName,
                SenderAvatar = "/images/default-avatar.png", // TODO: Get from UserProfile
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentType = message.AttachmentType,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                IsMine = message.SenderId == currentUserId,
                TimeAgo = GetTimeAgo(message.SentAt),
                FormattedTime = message.SentAt.ToString("HH:mm")
            };
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            if (timeSpan.TotalSeconds < 60)
                return "just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            else
                return dateTime.ToString("MMM dd");
        }
    }
}

