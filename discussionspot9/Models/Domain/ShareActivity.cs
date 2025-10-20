using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Tracks social sharing activity across the platform
    /// </summary>
    public class ShareActivity
    {
        [Key]
        public int ShareId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty; // post, community, profile, category
        
        [Required]
        public int ContentId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Platform { get; set; } = string.Empty; // facebook, twitter, linkedin, reddit, whatsapp, telegram, email, copy
        
        public string? UserId { get; set; } // Nullable for anonymous shares
        
        public DateTime SharedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [StringLength(2048)]
        public string? ReferrerUrl { get; set; }
        
        // Analytics fields
        public string? CountryCode { get; set; }
        public string? City { get; set; }
        public string? DeviceType { get; set; } // mobile, tablet, desktop
        public string? BrowserName { get; set; }
        public string? OsName { get; set; }
    }
}

