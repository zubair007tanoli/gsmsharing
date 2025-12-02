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
            var connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Track user presence for online users
                    await _presenceService.UserConnectedAsync(userId, connectionId);
                    
                    // Notify all users about this user coming online
                    await Clients.Others.SendAsync("UserOnline", userId);
                    
                    _logger.LogInformation($"[ChatHub] User {userId} connected (ConnectionId: {connectionId})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[ChatHub] Error tracking user presence for {userId} (ConnectionId: {connectionId})");
                    // Continue even if presence tracking fails - don't block SignalR connection
                }
            }
            else
            {
                _logger.LogWarning($"[ChatHub] Connection established without authenticated user (ConnectionId: {connectionId})");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // Remove user presence tracking
                    var isStillOnline = await _presenceService.UserDisconnectedAsync(userId, connectionId);
                    
                    // Only notify if user is completely offline (no other connections)
                    if (!isStillOnline)
                    {
                        await Clients.Others.SendAsync("UserOffline", userId);
                        _logger.LogInformation($"[ChatHub] User {userId} went offline (ConnectionId: {connectionId})");
                    }
                    else
                    {
                        _logger.LogInformation($"[ChatHub] User {userId} disconnected but still has other connections (ConnectionId: {connectionId})");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[ChatHub] Error removing user presence for {userId} (ConnectionId: {connectionId})");
                    // Continue even if presence tracking fails
                }
            }
            else
            {
                _logger.LogWarning($"[ChatHub] Disconnection without authenticated user (ConnectionId: {connectionId}, Exception: {exception?.Message})");
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

                // Save message to database
                var message = await _chatService.SendDirectMessageAsync(senderId, receiverId, content);

                // Create message objects (lightweight, no async operations)
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
                    IsMine = false,
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
                    IsMine = true,
                    message.TimeAgo,
                    message.FormattedTime
                };

                // Send to sender immediately (optimistic response)
                var senderTask = Clients.Caller.SendAsync("ReceiveDirectMessage", messageForSender);
                
                // Send to receiver in parallel (don't block on this)
                var receiverTask = Clients.User(receiverId).SendAsync("ReceiveDirectMessage", messageForReceiver);
                
                // Wait for sender delivery (user sees their message immediately)
                await senderTask;
                
                // Don't wait for receiver - let it deliver asynchronously
                _ = receiverTask.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, $"[ChatHub] Error delivering message {message.MessageId} to receiver");
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatHub] Error sending direct message from {Context.UserIdentifier} to {receiverId}: {ex.Message}");
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

                _logger.LogInformation($"[ChatHub] Sending room message from {senderId} to room {roomId}: {content.Substring(0, Math.Min(50, content.Length))}...");

                var message = await _chatService.SendRoomMessageAsync(senderId, roomId, content);

                _logger.LogInformation($"[ChatHub] Room message saved - MessageId: {message.MessageId}, SenderId: {senderId}, RoomId: {roomId}");

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

                _logger.LogInformation($"[ChatHub] Room message delivered - MessageId: {message.MessageId}, SenderId: {senderId}, RoomId: {roomId}");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"[ChatHub] Unauthorized room message attempt - User: {Context.UserIdentifier}, Room: {roomId}, Error: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "You are not a member of this room");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatHub] Error sending room message - User: {Context.UserIdentifier}, Room: {roomId}, Error: {ex.Message}");
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
                
                _logger.LogInformation($"[ChatHub] Message {messageId} marked as read by {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ChatHub] Error marking message {messageId} as read by {Context.UserIdentifier}");
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
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("JoinRoom called without authenticated user");
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                // Verify user is a member of the room in database
                var isMember = await _chatService.IsUserInRoomAsync(roomId, userId);
                if (!isMember)
                {
                    _logger.LogWarning($"User {userId} attempted to join room {roomId} but is not a member");
                    await Clients.Caller.SendAsync("Error", "You are not a member of this room");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
                
                // Notify room members (excluding the user who just joined)
                await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserJoinedRoom", userId, roomId);
                
                _logger.LogInformation($"[ChatHub] User {userId} joined room {roomId} (ConnectionId: {Context.ConnectionId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining room {roomId} for user {Context.UserIdentifier}");
                await Clients.Caller.SendAsync("Error", $"Failed to join room: {ex.Message}");
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
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("LeaveRoom called without authenticated user");
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
                
                // Notify room members (excluding the user who just left)
                await Clients.OthersInGroup($"room_{roomId}").SendAsync("UserLeftRoom", userId, roomId);
                
                _logger.LogInformation($"[ChatHub] User {userId} left room {roomId} (ConnectionId: {Context.ConnectionId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving room {roomId} for user {Context.UserIdentifier}");
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

