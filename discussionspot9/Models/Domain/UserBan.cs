using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Represents a site-wide or community-specific ban
    /// </summary>
    public class UserBan
    {
        [Key]
        public int BanId { get; set; }
        
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// If null, this is a site-wide ban. If set, ban is community-specific
        /// </summary>
        public int? CommunityId { get; set; }
        
        public string BannedByUserId { get; set; } = null!;
        
        public string Reason { get; set; } = null!;
        
        public DateTime BannedAt { get; set; }
        
        /// <summary>
        /// If null, ban is permanent. If set, ban expires at this time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsPermanent { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Type of ban: "site" or "community"
        /// </summary>
        public string BanType { get; set; } = "site"; // site, community
        
        /// <summary>
        /// Additional notes from moderator
        /// </summary>
        public string? ModeratorNotes { get; set; }
        
        /// <summary>
        /// If ban was appealed and lifted
        /// </summary>
        public DateTime? LiftedAt { get; set; }
        public string? LiftedByUserId { get; set; }
        public string? LiftReason { get; set; }
        
        // Navigation properties
        public virtual IdentityUser BannedUser { get; set; } = null!;
        public virtual IdentityUser BannedByUser { get; set; } = null!;
        public virtual IdentityUser? LiftedByUser { get; set; }
        public virtual Community? Community { get; set; }
    }
}

