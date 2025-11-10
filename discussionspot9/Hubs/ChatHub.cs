using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.ChatViewModels;

namespace discussionspot9.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatService chatService,
            IPresenceService presenceService,
            ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _presenceService = presenceService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Track user presence for online users
                    await _presenceService.UserConnectedAsync(userId, Context.ConnectionId);
                    
                    // Notify all users about this user coming online
                    await Clients.Others.SendAsync("UserOnline", userId);
                    
                    _logger.LogInformation($"User {userId} connected to ChatHub with connection {Context.ConnectionId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error tracking user presence for {userId} in ChatHub");
                    // Continue even if presence tracking fails - don't block SignalR connection
                }
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Remove user presence tracking
                    var isStillOnline = await _presenceService.UserDisconnectedAsync(userId, Context.ConnectionId);
                    
                    // Only notify if user is completely offline (no other connections)
                    if (!isStillOnline)
                    {
                        await Clients.Others.SendAsync("UserOffline", userId);
                    }
                    
                    _logger.LogInformation($"User {userId} disconnected from ChatHub");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error removing user presence for {userId} in ChatHub");
                    // Continue even if presence tracking fails
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send a direct message to another user
        /// </summary>
        public async Task SendDirectMessage(string receiverId, string content)
        {
            try
            {
                var senderId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(senderId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    await Clients.Caller.SendAsync("Error", "Message content cannot be empty");
                    return;
                }

                _logger.LogInformation($"Sending direct message from {senderId} to {receiverId}: {content.Substring(0, Math.Min(50, content.Length))}...");

                var message = await _chatService.SendDirectMessageAsync(senderId, receiverId, content);

                _logger.LogInformation($"Message saved with ID: {message.MessageId}, Content: {message.Content?.Substring(0, Math.Min(50, message.Content?.Length ?? 0))}");

                // Create message copies with proper IsMine flag for each recipient
                var messageForReceiver = new
                {
                    message.MessageId,
                    message.SenderId,
                    message.SenderName,
                    message.SenderAvatar,
                    message.ReceiverId,
                    message.Content,
                    message.AttachmentUrl,
                    message.AttachmentType,
                    message.SentAt,
                    message.IsRead,
                    IsMine = false, // Receiver didn't send this
                    message.TimeAgo,
                    message.FormattedTime
                };
                
                var messageForSender = new
                {
                    message.MessageId,
                    message.SenderId,
                    message.SenderName,
                    message.SenderAvatar,
                    message.ReceiverId,
                    message.Content,
                    message.AttachmentUrl,
                    message.AttachmentType,
                    message.SentAt,
                    message.IsRead,
                    IsMine = true, // Sender sent this
                    message.TimeAgo,
                    message.FormattedTime
                };

                // Send to receiver (if they're connected)
                await Clients.User(receiverId).SendAsync("ReceiveDirectMessage", messageForReceiver);
                
                // Send back to sender (for confirmation and real-time update)
                await Clients.Caller.SendAsync("ReceiveDirectMessage", messageForSender);

                _logger.LogInformation($"Direct message sent from {senderId} to {receiverId} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending direct message from {Context.UserIdentifier} to {receiverId}: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a message to a chat room
        /// </summary>
        public async Task SendRoomMessage(int roomId, string content)
        {
            try
            {
                var senderId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(senderId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    await Clients.Caller.SendAsync("Error", "Message content cannot be empty");
                    return;
                }

                _logger.LogInformation($"Sending room message from {senderId} to room {roomId}: {content.Substring(0, Math.Min(50, content.Length))}...");

                var message = await _chatService.SendRoomMessageAsync(senderId, roomId, content);

                _logger.LogInformation($"Room message saved with ID: {message.MessageId}, Content: {message.Content?.Substring(0, Math.Min(50, message.Content?.Length ?? 0))}");

                // Prepare message for all room members
                // Note: Each client will determine IsMine based on their own user ID vs SenderId
                var roomMessage = new
                {
                    message.MessageId,
                    message.SenderId,
                    message.SenderName,
                    message.SenderAvatar,
                    message.ReceiverId,
                    message.Content,
                    message.AttachmentUrl,
                    message.AttachmentType,
                    message.SentAt,
                    message.IsRead,
                    // Don't set IsMine here - client will determine based on SenderId
                    message.TimeAgo,
                    message.FormattedTime
                };

                // Send to all room members (including sender)
                // Each client receives the message and determines IsMine client-side
                await Clients.Group($"room_{roomId}").SendAsync("ReceiveRoomMessage", roomMessage);

                _logger.LogInformation($"Room message sent by {senderId} to room {roomId} successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "You are not a member of this room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending room message to room {roomId}: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        /// <summary>
        /// User is typing indicator
        /// </summary>
        public async Task UserTyping(string chatWithUserId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await _presenceService.UpdateTypingStatusAsync(userId, chatWithUserId, true);
                
                // Notify the other user
                await Clients.User(chatWithUserId).SendAsync("UserTyping", userId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating typing status");
            }
        }

        /// <summary>
        /// User stopped typing
        /// </summary>
        public async Task UserStoppedTyping(string chatWithUserId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await _presenceService.UpdateTypingStatusAsync(userId, chatWithUserId, false);
                
                // Notify the other user
                await Clients.User(chatWithUserId).SendAsync("UserTyping", userId, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stopped typing status");
            }
        }

        /// <summary>
        /// Mark message as read
        /// </summary>
        public async Task MarkAsRead(int messageId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await _chatService.MarkMessageAsReadAsync(messageId, userId);
                
                _logger.LogInformation($"Message {messageId} marked as read by {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking message {messageId} as read");
            }
        }

        /// <summary>
        /// Join a chat room
        /// </summary>
        public async Task JoinRoom(int roomId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
                
                // Notify room members
                await Clients.Group($"room_{roomId}").SendAsync("UserJoinedRoom", userId, roomId);
                
                _logger.LogInformation($"User {userId} joined room {roomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining room {roomId}");
            }
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        public async Task LeaveRoom(int roomId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
                
                // Notify room members
                await Clients.Group($"room_{roomId}").SendAsync("UserLeftRoom", userId, roomId);
                
                _logger.LogInformation($"User {userId} left room {roomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving room {roomId}");
            }
        }

        /// <summary>
        /// Get chat history
        /// </summary>
        public async Task<List<ChatMessageViewModel>> GetChatHistory(string otherUserId, int page = 1, int pageSize = 50)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return new List<ChatMessageViewModel>();

                return await _chatService.GetDirectChatHistoryAsync(userId, otherUserId, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history");
                return new List<ChatMessageViewModel>();
            }
        }

        /// <summary>
        /// Get room chat history
        /// </summary>
        public async Task<List<ChatMessageViewModel>> GetRoomHistory(int roomId, int page = 1, int pageSize = 50)
        {
            try
            {
                return await _chatService.GetRoomChatHistoryAsync(roomId, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting room {roomId} history");
                return new List<ChatMessageViewModel>();
            }
        }
    }
}

