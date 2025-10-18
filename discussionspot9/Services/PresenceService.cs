using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class PresenceService : IPresenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PresenceService> _logger;

        public PresenceService(ApplicationDbContext context, ILogger<PresenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UserConnectedAsync(string userId, string connectionId)
        {
            var presence = new UserPresence
            {
                UserId = userId,
                ConnectionId = connectionId,
                LastSeen = DateTime.UtcNow,
                Status = "online"
            };

            _context.UserPresences.Add(presence);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserDisconnectedAsync(string userId, string connectionId)
        {
            var presence = await _context.UserPresences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ConnectionId == connectionId);

            if (presence != null)
            {
                _context.UserPresences.Remove(presence);
                await _context.SaveChangesAsync();
            }

            // Check if user still has other connections
            var hasOtherConnections = await _context.UserPresences.AnyAsync(p => p.UserId == userId);
            return hasOtherConnections;
        }

        public async Task UpdateUserStatusAsync(string userId, string status)
        {
            var presences = await _context.UserPresences.Where(p => p.UserId == userId).ToListAsync();
            foreach (var presence in presences)
            {
                presence.Status = status;
                presence.LastSeen = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCurrentPageAsync(string userId, string currentPage)
        {
            var presences = await _context.UserPresences.Where(p => p.UserId == userId).ToListAsync();
            foreach (var presence in presences)
            {
                presence.CurrentPage = currentPage;
                presence.LastSeen = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTypingStatusAsync(string userId, string typingWithUserId, bool isTyping)
        {
            var presences = await _context.UserPresences.Where(p => p.UserId == userId).ToListAsync();
            foreach (var presence in presences)
            {
                presence.IsTyping = isTyping;
                presence.TypingInChatWith = isTyping ? typingWithUserId : null;
                presence.LastSeen = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<UserPresence?> GetUserPresenceAsync(string userId)
        {
            return await _context.UserPresences
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.LastSeen)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetOnlineUserIdsAsync()
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            return await _context.UserPresences
                .Where(p => p.LastSeen > fiveMinutesAgo)
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetUserStatusesAsync(List<string> userIds)
        {
            var presences = await _context.UserPresences
                .Where(p => userIds.Contains(p.UserId))
                .GroupBy(p => p.UserId)
                .Select(g => g.OrderByDescending(p => p.LastSeen).First())
                .ToListAsync();

            return presences.ToDictionary(p => p.UserId, p => p.Status);
        }

        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            return await _context.UserPresences.AnyAsync(p => p.UserId == userId && p.LastSeen > fiveMinutesAgo);
        }
    }
}

