using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Tracks user behavior for content recommendations
    /// </summary>
    public class UserActivity
    {
        [Key]
        public long Id { get; set; }
        
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        
        public int? PostId { get; set; }
        public int? CommunityId { get; set; }
        
        [StringLength(50)]
        public string ActivityType { get; set; } = string.Empty; // View, Click, Comment, Share, Save, Like
        
        public int TimeSpentSeconds { get; set; }
        public int ScrollDepthPercent { get; set; }
        
        [StringLength(500)]
        public string? Referrer { get; set; }
        
        [StringLength(200)]
        public string? DeviceType { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime ActivityAt { get; set; }
        
        // Additional context (JSON)
        public string? Metadata { get; set; }
        
        // Navigation
        public virtual Post? Post { get; set; }
        public virtual Community? Community { get; set; }
    }
}

