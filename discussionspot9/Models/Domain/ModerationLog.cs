using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Logs all moderation actions for audit trail
    /// </summary>
    public class ModerationLog
    {
        [Key]
        public long LogId { get; set; }
        
        public string ModeratorId { get; set; } = null!;
        
        public string TargetUserId { get; set; } = null!;
        
        public int? CommunityId { get; set; }
        
        /// <summary>
        /// Type of action: "ban", "unban", "role_change", "delete_post", "delete_comment", etc.
        /// </summary>
        public string ActionType { get; set; } = null!;
        
        /// <summary>
        /// Entity affected: "user", "post", "comment", "community"
        /// </summary>
        public string EntityType { get; set; } = null!;
        
        /// <summary>
        /// ID of the affected entity (postId, commentId, etc.)
        /// </summary>
        public string? EntityId { get; set; }
        
        public string Reason { get; set; } = null!;
        
        public DateTime PerformedAt { get; set; }
        
        /// <summary>
        /// Old value (e.g., old role, old status)
        /// </summary>
        public string? OldValue { get; set; }
        
        /// <summary>
        /// New value (e.g., new role, new status)
        /// </summary>
        public string? NewValue { get; set; }
        
        /// <summary>
        /// IP address of moderator for security
        /// </summary>
        public string? ModeratorIp { get; set; }
        
        /// <summary>
        /// Additional metadata in JSON format
        /// </summary>
        public string? Metadata { get; set; }
        
        // Navigation properties
        public virtual IdentityUser Moderator { get; set; } = null!;
        public virtual IdentityUser TargetUser { get; set; } = null!;
        public virtual Community? Community { get; set; }
    }
}

