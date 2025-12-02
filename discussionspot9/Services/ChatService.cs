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
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepository,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<ChatMessageViewModel> SendDirectMessageAsync(string senderId, string receiverId, string content, string? attachmentUrl = null)
        {
            try
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

                // Save message and get sender name in parallel for better performance
                var saveTask = _chatRepository.AddMessageAsync(message);
                
                // Get sender name using a separate DbContext instance to avoid conflicts
                string senderName = "User";
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    var senderNameResult = await context.UserProfiles
                        .AsNoTracking()
                        .Where(p => p.UserId == senderId)
                        .Select(p => p.DisplayName)
                        .FirstOrDefaultAsync();
                    senderName = senderNameResult ?? "User";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[ChatService] Could not fetch sender name for {senderId}, using default");
                }
                
                var savedMessage = await saveTask;
                
                return MapToViewModel(savedMessage, senderId, senderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatService] Error sending direct message from {senderId} to {receiverId}: {ex.Message}");
                throw;
            }
        }

        public async Task<ChatMessageViewModel> SendRoomMessageAsync(string senderId, int roomId, string content, string? attachmentUrl = null)
        {
            try
            {
                // Check if user is in room
                var isInRoom = await _chatRepository.IsUserInRoomAsync(roomId, senderId);
                if (!isInRoom)
                {
                    _logger.LogWarning($"[ChatService] User {senderId} is not a member of room {roomId}");
                    throw new UnauthorizedAccessException("User is not a member of this room");
                }

                var message = new ChatMessage
                {
                    SenderId = senderId,
                    ChatRoomId = roomId,
                    ReceiverId = null, // Room messages don't have a receiver
                    Content = content,
                    AttachmentUrl = attachmentUrl,
                    SentAt = DateTime.UtcNow,
                    IsRead = false, // Room messages are always considered read when sent
                    IsDeleted = false
                };

                var savedMessage = await _chatRepository.AddMessageAsync(message);
                
                // Get sender info from UserProfiles using factory to avoid concurrency issues
                string senderName = "Unknown";
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    var sender = await context.UserProfiles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.UserId == senderId);
                    senderName = sender?.DisplayName ?? "Unknown";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[ChatService] Could not fetch sender name for {senderId}, using default");
                }
                
                _logger.LogInformation($"[ChatService] Room message saved - MessageId: {savedMessage.MessageId}, SenderId: {senderId}, RoomId: {roomId}");
                
                return MapToViewModel(savedMessage, senderId, senderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatService] Error sending room message from {senderId} to room {roomId}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ChatMessageViewModel>> GetDirectChatHistoryAsync(string userId, string otherUserId, int page = 1, int pageSize = 50)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var messages = await _chatRepository.GetDirectMessagesAsync(userId, otherUserId, skip, pageSize);
                
                // Get user profiles for sender names using factory
                var userIds = messages.Select(m => m.SenderId).Distinct().ToList();
                Dictionary<string, string> userProfiles = new();
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    userProfiles = await context.UserProfiles
                        .AsNoTracking()
                        .Where(p => userIds.Contains(p.UserId))
                        .ToDictionaryAsync(p => p.UserId, p => p.DisplayName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[ChatService] Could not fetch user profiles, using defaults");
                }
                
                var viewModels = messages.Select(m => 
                {
                    var senderName = userProfiles.GetValueOrDefault(m.SenderId, "Unknown");
                    return MapToViewModel(m, userId, senderName);
                }).Reverse().ToList();
                
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatService] Error getting direct chat history for {userId} and {otherUserId}: {ex.Message}");
                return new List<ChatMessageViewModel>();
            }
        }

        public async Task<List<ChatMessageViewModel>> GetRoomChatHistoryAsync(int roomId, int page = 1, int pageSize = 50)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var messages = await _chatRepository.GetRoomMessagesAsync(roomId, skip, pageSize);
                
                // Get user profiles for sender names using factory
                var userIds = messages.Select(m => m.SenderId).Distinct().ToList();
                Dictionary<string, string> userProfiles = new();
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    userProfiles = await context.UserProfiles
                        .AsNoTracking()
                        .Where(p => userIds.Contains(p.UserId))
                        .ToDictionaryAsync(p => p.UserId, p => p.DisplayName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[ChatService] Could not fetch user profiles, using defaults");
                }
                
                var viewModels = messages.Select(m => 
                {
                    var senderName = userProfiles.GetValueOrDefault(m.SenderId, "Unknown");
                    return MapToViewModel(m, m.SenderId, senderName);
                }).Reverse().ToList();
                
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatService] Error getting room chat history for room {roomId}: {ex.Message}");
                return new List<ChatMessageViewModel>();
            }
        }

        public async Task<List<DirectChatViewModel>> GetUserDirectChatsAsync(string userId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                
                // Get all messages where user is sender or receiver
                var recentChats = await context.ChatMessages
                    .AsNoTracking()
                    .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted && m.ChatRoomId == null)
                    .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                    .Select(g => new
                    {
                        OtherUserId = g.Key,
                        LastMessage = g.OrderByDescending(m => m.SentAt).FirstOrDefault(),
                        UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                    })
                    .ToListAsync();

                // Get all user profiles for the other users
                var otherUserIds = recentChats.Where(c => !string.IsNullOrEmpty(c.OtherUserId)).Select(c => c.OtherUserId!).ToList();
                var userProfiles = await context.UserProfiles
                    .AsNoTracking()
                    .Where(p => otherUserIds.Contains(p.UserId))
                    .ToDictionaryAsync(p => p.UserId, p => p);

                var chats = new List<DirectChatViewModel>();
                foreach (var chat in recentChats)
                {
                    if (string.IsNullOrEmpty(chat.OtherUserId)) continue;
                    
                    if (!userProfiles.TryGetValue(chat.OtherUserId, out var otherUser)) continue;

                    // Get sender name for last message
                    string lastMessageSenderName = "Unknown";
                    if (chat.LastMessage != null)
                    {
                        var senderId = chat.LastMessage.SenderId;
                        if (userProfiles.TryGetValue(senderId, out var senderProfile))
                        {
                            lastMessageSenderName = senderProfile.DisplayName;
                        }
                    }

                    chats.Add(new DirectChatViewModel
                    {
                        UserId = chat.OtherUserId,
                        DisplayName = otherUser.DisplayName,
                        AvatarUrl = otherUser.AvatarUrl ?? "/images/default-avatar.png",
                        Status = "online", // Will be updated by PresenceService
                        UnreadCount = chat.UnreadCount,
                        LastMessage = chat.LastMessage != null ? MapToViewModel(chat.LastMessage, userId, lastMessageSenderName) : null
                    });
                }

                return chats.OrderByDescending(c => c.LastMessage?.SentAt ?? DateTime.MinValue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatService] Error getting user direct chats for {userId}: {ex.Message}");
                return new List<DirectChatViewModel>();
            }
        }

        public async Task<List<ChatRoomViewModel>> GetUserChatRoomsAsync(string userId)
        {
            try
            {
                var rooms = await _chatRepository.GetUserRoomsAsync(userId);
                
                // Get last message for each room
                var roomIds = rooms.Select(r => r.ChatRoomId).ToList();
                var lastMessages = new Dictionary<int, ChatMessage>();
                
                Dictionary<string, string> userProfiles = new();
                if (roomIds.Any())
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    var messages = await context.ChatMessages
                        .AsNoTracking()
                        .Where(m => roomIds.Contains(m.ChatRoomId ?? 0) && !m.IsDeleted)
                        .OrderByDescending(m => m.SentAt)
                        .GroupBy(m => m.ChatRoomId)
                        .Select(g => g.First())
                        .ToListAsync();
                    
                    foreach (var msg in messages)
                    {
                        if (msg.ChatRoomId.HasValue)
                        {
                            lastMessages[msg.ChatRoomId.Value] = msg;
                        }
                    }

                    // Get sender names for last messages
                    var senderIds = lastMessages.Values.Select(m => m.SenderId).Distinct().ToList();
                    if (senderIds.Any())
                    {
                        userProfiles = await context.UserProfiles
                            .AsNoTracking()
                            .Where(p => senderIds.Contains(p.UserId))
                            .ToDictionaryAsync(p => p.UserId, p => p.DisplayName);
                    }
                }

                var viewModels = rooms.Select(r => 
                {
                    var viewModel = new ChatRoomViewModel
                    {
                        ChatRoomId = r.ChatRoomId,
                        Name = r.Name,
                        Description = r.Description,
                        IconUrl = r.IconUrl,
                        MemberCount = r.MemberCount,
                        IsPublic = r.IsPublic,
                        UnreadCount = 0 // TODO: Calculate unread count
                    };

                    // Add last message if exists
                    if (lastMessages.TryGetValue(r.ChatRoomId, out var lastMsg))
                    {
                        var senderName = userProfiles.GetValueOrDefault(lastMsg.SenderId, "Unknown");
                        viewModel.LastMessage = MapToViewModel(lastMsg, userId, senderName);
                    }

                    return viewModel;
                }).ToList();

                _logger.LogInformation($"[ChatService] Retrieved {viewModels.Count} chat rooms for user {userId}");
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user chat rooms for {userId}");
                return new List<ChatRoomViewModel>();
            }
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

        public async Task<bool> IsUserInRoomAsync(int roomId, string userId)
        {
            return await _chatRepository.IsUserInRoomAsync(roomId, userId);
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
                Content = message.Content ?? "",
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

