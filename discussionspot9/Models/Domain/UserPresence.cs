using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class UserPresence
    {
        public int PresenceId { get; set; }
        public string UserId { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;
        public DateTime LastSeen { get; set; }
        public string Status { get; set; } = "online"; // online, away, busy, offline
        public string? CurrentPage { get; set; }
        public string? DeviceInfo { get; set; }
        public bool IsTyping { get; set; }
        public string? TypingInChatWith { get; set; } // UserId they're chatting with

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
    }
}

