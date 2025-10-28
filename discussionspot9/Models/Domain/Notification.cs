using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    public class Notification
    {
        public int NotificationId { get; set; }
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Message { get; set; }
        
        [MaxLength(50)]
        public string? EntityType { get; set; }
        
        [MaxLength(450)]
        public string? EntityId { get; set; }
        
        public bool IsRead { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Enhanced fields for better notifications
        [MaxLength(450)]
        public string? ActorUserId { get; set; } // Who triggered this notification
        
        [MaxLength(100)]
        public string? ActorDisplayName { get; set; }
        
        [MaxLength(2048)]
        public string? ActorAvatarUrl { get; set; }
        
        [MaxLength(2048)]
        public string? Url { get; set; } // Direct link to content
        
        public bool EmailSent { get; set; } = false;
        
        public DateTime? EmailSentAt { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        [MaxLength(100)]
        public string? GroupId { get; set; } // For grouping related notifications

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual IdentityUser? Actor { get; set; }
    }
}
