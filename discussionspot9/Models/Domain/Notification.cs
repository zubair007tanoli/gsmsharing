using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public bool IsRead { get; set; }
        public string? Url { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
    }
}
