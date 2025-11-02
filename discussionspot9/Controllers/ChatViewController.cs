using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.ChatViewModels;
using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    [Authorize]
    public class ChatViewController : Controller
    {
        private readonly IChatService _chatService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatViewController> _logger;

        public ChatViewController(
            IChatService chatService, 
            ApplicationDbContext context,
            ILogger<ChatViewController> logger)
        {
            _chatService = chatService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Main chat inbox page
        /// GET: /chat
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                var directChats = await _chatService.GetUserDirectChatsAsync(userId);
                var rooms = await _chatService.GetUserChatRoomsAsync(userId);
                var unreadCount = await _chatService.GetUnreadCountAsync(userId);

                var model = new ChatInboxViewModel
                {
                    DirectChats = directChats,
                    ChatRooms = rooms,
                    UnreadCount = unreadCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat inbox");
                return View(new ChatInboxViewModel());
            }
        }

        /// <summary>
        /// Direct message conversation page
        /// GET: /chat/direct/{userId}
        /// </summary>
        public async Task<IActionResult> Direct(string userId, [FromQuery] int page = 1)
        {
            try
            {
                var currentUserId = GetUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                var messages = await _chatService.GetDirectChatHistoryAsync(currentUserId, userId, page, 50);
                
                // Get user profile info for display
                var userProfile = await _context.UserProfiles.FindAsync(userId);
                
                var model = new DirectChatPageViewModel
                {
                    UserId = userId,
                    UserName = userProfile?.DisplayName ?? "User",
                    AvatarUrl = userProfile?.AvatarUrl ?? "/images/default-avatar.png",
                    Messages = messages,
                    CurrentPage = page
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading direct chat with {userId}");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Chat room page
        /// GET: /chat/rooms/{roomId}
        /// </summary>
        public async Task<IActionResult> Room(int roomId, [FromQuery] int page = 1)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Auth", "Account");
                }

                var messages = await _chatService.GetRoomChatHistoryAsync(roomId, page, 50);
                var rooms = await _chatService.GetUserChatRoomsAsync(userId);
                var room = rooms.FirstOrDefault(r => r.ChatRoomId == roomId);

                if (room == null)
                {
                    return NotFound();
                }

                var model = new RoomChatViewModel
                {
                    Room = room,
                    Messages = messages,
                    CurrentPage = page
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading chat room {roomId}");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create room page
        /// GET: /chat/rooms/create
        /// </summary>
        [HttpGet]
        public IActionResult CreateRoom()
        {
            var model = new CreateChatRoomViewModel();
            return View(model);
        }

        /// <summary>
        /// Create room - POST
        /// POST: /chat/rooms/create
        /// </summary>
        [HttpPost("chat/rooms/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(CreateChatRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
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

                TempData["SuccessMessage"] = "Chat room created successfully!";
                return RedirectToAction("Room", new { roomId = room.ChatRoomId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room");
                ModelState.AddModelError("", "Failed to create chat room. Please try again.");
                return View(model);
            }
        }
    }
}

