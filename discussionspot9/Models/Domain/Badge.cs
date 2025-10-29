using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Represents an achievement badge that can be earned by users
    /// </summary>
    public class Badge
    {
        [Key]
        public int BadgeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(500)]
        public string Description { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = null!; // Activity, Quality, Community, Special
        
        [Required]
        [StringLength(50)]
        public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
        
        [StringLength(2048)]
        public string? IconUrl { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IconClass { get; set; } = "fas fa-trophy"; // Font Awesome class
        
        [Required]
        [StringLength(20)]
        public string Color { get; set; } = "#3b82f6"; // Badge color (hex)
        
        public int SortOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
        
        // Computed properties
        [NotMapped]
        public int TimesAwarded => UserBadges?.Count ?? 0;
        
        [NotMapped]
        public string RarityEmoji => Rarity switch
        {
            "Common" => "⚪",
            "Rare" => "🔵",
            "Epic" => "🟣",
            "Legendary" => "🟡",
            _ => "⚪"
        };
    }

    /// <summary>
    /// Tracks which badges users have earned
    /// </summary>
    public class UserBadge
    {
        [Key]
        public int UserBadgeId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;
        
        public int BadgeId { get; set; }
        
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string? EarnedReason { get; set; }
        
        public bool IsDisplayed { get; set; } = true; // User can hide badges
        
        public int DisplayOrder { get; set; } // Order on profile (1, 2, 3 for showcase)
        
        public bool IsNotified { get; set; } = false; // Track if user was notified
        
        // Navigation properties
        public virtual Badge Badge { get; set; } = null!;
    }

    /// <summary>
    /// Defines conditions for earning badges automatically
    /// </summary>
    public class BadgeRequirement
    {
        [Key]
        public int BadgeRequirementId { get; set; }
        
        public int BadgeId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RequirementType { get; set; } = null!;
        // Types: PostCount, CommentCount, Karma, VoteCount, AwardCount, DaysActive, etc.
        
        public int RequiredValue { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual Badge Badge { get; set; } = null!;
    }
}

