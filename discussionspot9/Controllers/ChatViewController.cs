using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.ChatViewModels;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatViewController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<ChatViewController> _logger;

        public ChatViewController(
            IChatService chatService,
            IPresenceService presenceService,
            ILogger<ChatViewController> logger)
        {
            _chatService = chatService;
            _presenceService = presenceService;
            _logger = logger;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Chat inbox page
        /// GET: /ChatView or /ChatView/Index
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                var model = new ChatInboxViewModel
                {
                    DirectChats = await _chatService.GetUserDirectChatsAsync(userId),
                    ChatRooms = await _chatService.GetUserChatRoomsAsync(userId),
                    UnreadCount = await _chatService.GetUnreadCountAsync(userId)
                };

                return View("~/Views/Chat/Index.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat inbox");
                return View("Error");
            }
        }

        /// <summary>
        /// Direct chat page
        /// GET: /ChatView/Direct/{userId}
        /// </summary>
        [HttpGet("Direct/{otherUserId}")]
        public async Task<IActionResult> Direct(string otherUserId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                // Get chat history
                var messages = await _chatService.GetDirectChatHistoryAsync(userId, otherUserId);
                
                // Get other user info (you'll need to implement this or get from UserProfiles)
                var model = new DirectChatPageViewModel
                {
                    OtherUserId = otherUserId,
                    Messages = messages,
                    CurrentUserId = userId
                };

                return View("~/Views/Chat/Direct.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading direct chat with {otherUserId}");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Chat room page
        /// GET: /ChatView/Room/{roomId}
        /// </summary>
        [HttpGet("Room/{roomId:int}")]
        public async Task<IActionResult> Room(int roomId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                // Check if user is member of room
                var isMember = await _chatService.IsUserInRoomAsync(roomId, userId);
                if (!isMember)
                {
                    TempData["Error"] = "You are not a member of this room.";
                    return RedirectToAction("Index");
                }

                // Get room messages
                var messages = await _chatService.GetRoomChatHistoryAsync(roomId);
                
                // Get room info
                var rooms = await _chatService.GetUserChatRoomsAsync(userId);
                var room = rooms.FirstOrDefault(r => r.ChatRoomId == roomId);

                if (room == null)
                {
                    TempData["Error"] = "Room not found.";
                    return RedirectToAction("Index");
                }

                var model = new RoomChatPageViewModel
                {
                    Room = room,
                    Messages = messages,
                    CurrentUserId = userId
                };

                return View("~/Views/Chat/Room.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading chat room {roomId}");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create room page
        /// GET: /ChatView/CreateRoom
        /// </summary>
        [HttpGet("CreateRoom")]
        public IActionResult CreateRoom()
        {
            return View("~/Views/Chat/CreateRoom.cshtml");
        }

        /// <summary>
        /// Create room POST
        /// POST: /ChatView/CreateRoom
        /// </summary>
        [HttpPost("CreateRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(CreateChatRoomViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Chat/CreateRoom.cshtml", model);
                }

                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                var room = await _chatService.CreateChatRoomAsync(
                    userId,
                    model.Name,
                    model.Description,
                    model.IsPublic
                );

                TempData["Success"] = "Chat room created successfully!";
                return RedirectToAction("Room", new { roomId = room.ChatRoomId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                ModelState.AddModelError("", "Failed to create chat room. Please try again.");
                return View("~/Views/Chat/CreateRoom.cshtml", model);
            }
        }
    }
}
