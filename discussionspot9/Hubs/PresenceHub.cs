using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using discussionspot9.Interfaces;

namespace discussionspot9.Hubs
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly IPresenceService _presenceService;
        private readonly ILogger<PresenceHub> _logger;

        public PresenceHub(
            IPresenceService presenceService,
            ILogger<PresenceHub> logger)
        {
            _presenceService = presenceService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await _presenceService.UserConnectedAsync(userId, Context.ConnectionId);
                
                // Send current user status to caller
                var presence = await _presenceService.GetUserPresenceAsync(userId);
                await Clients.Caller.SendAsync("CurrentPresence", presence);
                
                // Notify all users about this user
                await Clients.Others.SendAsync("UserStatusChanged", userId, "online");
                
                _logger.LogInformation($"User {userId} connected to PresenceHub");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                var isStillOnline = await _presenceService.UserDisconnectedAsync(userId, Context.ConnectionId);
                
                if (!isStillOnline)
                {
                    // User is completely offline (no more connections)
                    await Clients.Others.SendAsync("UserStatusChanged", userId, "offline");
                }
                
                _logger.LogInformation($"User {userId} disconnected from PresenceHub");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Update user status (online, away, busy)
        /// </summary>
        public async Task UpdateStatus(string status)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await _presenceService.UpdateUserStatusAsync(userId, status);
                
                // Notify all users about status change
                await Clients.All.SendAsync("UserStatusChanged", userId, status);
                
                _logger.LogInformation($"User {userId} status updated to {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status");
            }
        }

        /// <summary>
        /// Update current page/location
        /// </summary>
        public async Task UpdateCurrentPage(string currentPage)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (string.IsNullOrEmpty(userId)) return;

                await _presenceService.UpdateCurrentPageAsync(userId, currentPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current page");
            }
        }

        /// <summary>
        /// Get online users
        /// </summary>
        public async Task<List<string>> GetOnlineUsers()
        {
            try
            {
                return await _presenceService.GetOnlineUserIdsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return new List<string>();
            }
        }
    }
}

