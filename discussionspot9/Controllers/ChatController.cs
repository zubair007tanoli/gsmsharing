using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.ChatViewModels;
using discussionspot9.Data.DbContext;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<ChatController> _logger;
        private readonly ApplicationDbContext _context;

        public ChatController(
            IChatService chatService, 
            IPresenceService presenceService,
            ILogger<ChatController> logger,
            ApplicationDbContext context)
        {
            _chatService = chatService;
            _presenceService = presenceService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Get user's direct chats
        /// GET: /api/chat/direct
        /// </summary>
        [HttpGet("direct")]
        public async Task<IActionResult> GetDirectChats()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var chats = await _chatService.GetUserDirectChatsAsync(userId);
                return Ok(new { success = true, chats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting direct chats");
                return StatusCode(500, new { success = false, message = "Failed to get direct chats" });
            }
        }

        /// <summary>
        /// Get user's chat rooms
        /// GET: /api/chat/rooms
        /// </summary>
        [HttpGet("rooms")]
        public async Task<IActionResult> GetUserChatRooms()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var rooms = await _chatService.GetUserChatRoomsAsync(userId);
                return Ok(new { success = true, rooms });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user chat rooms");
                return StatusCode(500, new { success = false, message = "Failed to get chat rooms" });
            }
        }

        /// <summary>
        /// Create a new chat room
        /// POST: /api/chat/rooms
        /// </summary>
        [HttpPost("rooms")]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid model", errors = ModelState });
                }

                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var room = await _chatService.CreateChatRoomAsync(
                    userId,
                    model.Name,
                    model.Description,
                    model.IsPublic
                );

                return Ok(new { success = true, room });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                return StatusCode(500, new { success = false, message = "Failed to create chat room" });
            }
        }

        /// <summary>
        /// Join a chat room
        /// POST: /api/chat/rooms/{roomId}/join
        /// </summary>
        [HttpPost("rooms/{roomId}/join")]
        public async Task<IActionResult> JoinChatRoom(int roomId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                await _chatService.JoinChatRoomAsync(roomId, userId);
                return Ok(new { success = true, message = "Joined chat room successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat room {roomId}");
                return StatusCode(500, new { success = false, message = "Failed to join chat room" });
            }
        }

        /// <summary>
        /// Leave a chat room
        /// POST: /api/chat/rooms/{roomId}/leave
        /// </summary>
        [HttpPost("rooms/{roomId}/leave")]
        public async Task<IActionResult> LeaveChatRoom(int roomId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                await _chatService.LeaveChatRoomAsync(roomId, userId);
                return Ok(new { success = true, message = "Left chat room successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat room {roomId}");
                return StatusCode(500, new { success = false, message = "Failed to leave chat room" });
            }
        }

        /// <summary>
        /// Get chat history for a direct chat
        /// GET: /api/chat/direct/{otherUserId}/history?page=1&pageSize=50
        /// </summary>
        [HttpGet("direct/{otherUserId}/history")]
        public async Task<IActionResult> GetDirectChatHistory(string otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var messages = await _chatService.GetDirectChatHistoryAsync(userId, otherUserId, page, pageSize);
                return Ok(new { success = true, messages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat history with {otherUserId}");
                return StatusCode(500, new { success = false, message = "Failed to get chat history" });
            }
        }

        /// <summary>
        /// Get chat history for a room
        /// GET: /api/chat/rooms/{roomId}/history?page=1&pageSize=50
        /// </summary>
        [HttpGet("rooms/{roomId}/history")]
        public async Task<IActionResult> GetRoomChatHistory(int roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var messages = await _chatService.GetRoomChatHistoryAsync(roomId, page, pageSize);
                return Ok(new { success = true, messages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting room {roomId} chat history");
                return StatusCode(500, new { success = false, message = "Failed to get chat history" });
            }
        }

        /// <summary>
        /// Get unread message count
        /// GET: /api/chat/unread-count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var count = await _chatService.GetUnreadCountAsync(userId);
                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { success = false, message = "Failed to get unread count" });
            }
        }

        /// <summary>
        /// Mark message as read
        /// POST: /api/chat/messages/{messageId}/read
        /// </summary>
        [HttpPost("messages/{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int messageId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                await _chatService.MarkMessageAsReadAsync(messageId, userId);
                return Ok(new { success = true, message = "Message marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking message {messageId} as read");
                return StatusCode(500, new { success = false, message = "Failed to mark message as read" });
            }
        }

        /// <summary>
        /// Get online users
        /// GET: /api/chat/online-users
        /// </summary>
        [HttpGet("online-users")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                // Get online user IDs
                var onlineUserIds = await _presenceService.GetOnlineUserIdsAsync();
                
                if (!onlineUserIds.Any())
                {
                    return Ok(new { success = true, count = 0, users = new List<object>() });
                }

                // Exclude current user and get user profiles
                var otherUserIds = onlineUserIds.Where(id => id != userId).ToList();
                
                var onlineUsers = await _context.UserProfiles
                    .Where(u => otherUserIds.Contains(u.UserId))
                    .AsNoTracking()
                    .OrderByDescending(u => u.LastActive)
                    .Take(50)
                    .Select(u => new
                    {
                        userId = u.UserId,
                        displayName = u.DisplayName,
                        avatarUrl = u.AvatarUrl,
                        lastSeen = u.LastActive,
                        status = "online"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = onlineUsers.Count,
                    users = onlineUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return StatusCode(500, new { success = false, message = "Failed to get online users" });
            }
        }
    }
}

