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
                // TEMPORARILY DISABLED: UserPresences table doesn't exist - blocking SignalR
                // await _presenceService.UserConnectedAsync(userId, Context.ConnectionId);
                
                // Notify all users about this user coming online
                await Clients.Others.SendAsync("UserOnline", userId);
                
                _logger.LogInformation($"User {userId} connected to ChatHub with connection {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // TEMPORARILY DISABLED: UserPresences table doesn't exist - blocking SignalR
                // await _presenceService.UserDisconnectedAsync(userId, Context.ConnectionId);
                
                // Notify all users about this user going offline
                await Clients.Others.SendAsync("UserOffline", userId);
                
                _logger.LogInformation($"User {userId} disconnected from ChatHub");
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

                var message = await _chatService.SendDirectMessageAsync(senderId, receiverId, content);

                // Send to receiver
                await Clients.User(receiverId).SendAsync("ReceiveDirectMessage", message);
                
                // Send back to sender (for confirmation)
                await Clients.Caller.SendAsync("MessageSent", message);

                _logger.LogInformation($"Direct message sent from {senderId} to {receiverId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending direct message");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
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

                var message = await _chatService.SendRoomMessageAsync(senderId, roomId, content);

                // Send to all room members
                await Clients.Group($"room_{roomId}").SendAsync("ReceiveRoomMessage", message);

                _logger.LogInformation($"Room message sent by {senderId} to room {roomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending room message to room {roomId}");
                await Clients.Caller.SendAsync("Error", "Failed to send message to room");
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

