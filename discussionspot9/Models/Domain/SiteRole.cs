using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Represents a site-wide role assigned to a user (SiteAdmin, Moderator, etc.)
    /// </summary>
    public class SiteRole
    {
        [Key]
        public int RoleId { get; set; }
        
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Role name: "SiteAdmin", "Moderator", etc.
        /// </summary>
        public string RoleName { get; set; } = null!;
        
        public DateTime AssignedAt { get; set; }
        
        public string AssignedByUserId { get; set; } = null!;
        
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Optional notes about why this role was assigned
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// If role was removed
        /// </summary>
        public DateTime? RemovedAt { get; set; }
        public string? RemovedByUserId { get; set; }
        
        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual IdentityUser AssignedBy { get; set; } = null!;
        public virtual IdentityUser? RemovedBy { get; set; }
    }
}
