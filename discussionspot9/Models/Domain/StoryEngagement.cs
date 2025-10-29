using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Tracks user reactions to stories (like, love, wow, etc.)
    /// </summary>
    public class StoryReaction
    {
        public int StoryReactionId { get; set; }
        
        [Required]
        public int StoryId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;
        
        [Required]
        [StringLength(20)]
        public string ReactionType { get; set; } = null!; // like, love, wow, sad, laugh
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Story Story { get; set; } = null!;
    }

    /// <summary>
    /// Tracks story shares across platforms
    /// </summary>
    public class StoryShare
    {
        public int StoryShareId { get; set; }
        
        [Required]
        public int StoryId { get; set; }
        
        [StringLength(450)]
        public string? UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Platform { get; set; } = null!; // twitter, facebook, whatsapp, copy_link, etc.
        
        public DateTime SharedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        // Navigation properties
        public virtual Story Story { get; set; } = null!;
    }

    /// <summary>
    /// Analytics for story performance
    /// </summary>
    public class StoryAnalytics
    {
        [Key]
        public int StoryId { get; set; }
        
        public int ViewCount { get; set; }
        public int UniqueViewCount { get; set; }
        public int CompletionCount { get; set; } // Users who viewed all slides
        public int ShareCount { get; set; }
        public int ReactionCount { get; set; }
        public int AverageViewDuration { get; set; } // in milliseconds
        public double CompletionRate { get; set; } // percentage
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Reaction breakdown
        public int LikeCount { get; set; }
        public int LoveCount { get; set; }
        public int WowCount { get; set; }
        public int SadCount { get; set; }
        public int LaughCount { get; set; }
        
        // Navigation properties
        public virtual Story Story { get; set; } = null!;
        
        // Computed properties
        [NotMapped]
        public bool IsPopular => ViewCount > 1000;
        
        [NotMapped]
        public bool IsViral => ShareCount > 100;
        
        [NotMapped]
        public string PerformanceRating
        {
            get
            {
                if (CompletionRate > 80 && ViewCount > 500) return "Excellent 🌟";
                if (CompletionRate > 60 && ViewCount > 100) return "Good 👍";
                if (CompletionRate > 40) return "Average 📊";
                return "Needs Improvement 📉";
            }
        }
    }

    /// <summary>
    /// Tracks individual story views for analytics
    /// </summary>
    public class StoryView
    {
        public int StoryViewId { get; set; }
        
        [Required]
        public int StoryId { get; set; }
        
        [StringLength(450)]
        public string? UserId { get; set; } // Null for anonymous views
        
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
        
        public int SlidesViewed { get; set; } // How many slides user viewed
        
        public int TimeSpent { get; set; } // in milliseconds
        
        public bool Completed { get; set; } // Viewed all slides
        
        [StringLength(100)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [StringLength(100)]
        public string? ReferrerUrl { get; set; }
        
        // Navigation properties
        public virtual Story Story { get; set; } = null!;
    }
}

