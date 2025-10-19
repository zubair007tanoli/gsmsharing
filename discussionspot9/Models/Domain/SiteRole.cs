using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Site-wide roles for users (Admin, Moderator, etc.)
    /// Separate from ASP.NET Identity roles for flexibility
    /// </summary>
    public class SiteRole
    {
        [Key]
        public int SiteRoleId { get; set; }
        
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Role name: "SiteAdmin", "Moderator", "Verified", "VIP"
        /// </summary>
        public string RoleName { get; set; } = null!;
        
        public DateTime AssignedAt { get; set; }
        
        public string? AssignedByUserId { get; set; }
        
        /// <summary>
        /// If null, role is permanent. Otherwise, role expires
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Additional permissions in JSON format
        /// </summary>
        public string? Permissions { get; set; }
        
        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual IdentityUser? AssignedByUser { get; set; }
    }
}

