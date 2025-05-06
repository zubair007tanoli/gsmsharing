using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents an award given to a post
    /// </summary>
    public class PostAward
    {
        [Key]
        public int PostAwardId { get; set; }

        [Required]
        [Display(Name = "Post")]
        public int PostId { get; set; }

        [Required]
        [Display(Name = "Award")]
        public int AwardId { get; set; }

        [Display(Name = "Awarded By")]
        public string? AwardedByUserId { get; set; }

        [Display(Name = "Award Date")]
        public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        [Display(Name = "Award Message")]
        public string? Message { get; set; }

        [Display(Name = "Anonymous")]
        public bool IsAnonymous { get; set; } = false;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("AwardId")]
        public virtual Award Award { get; set; } = null!;

        [ForeignKey("AwardedByUserId")]
        public virtual ApplicationUsers? AwardedByUser { get; set; }

        // Helper properties
        [NotMapped]
        public string AwardedByDisplayName
        {
            get
            {
                if (IsAnonymous) return "Anonymous";
                if (AwardedByUser == null) return "Unknown";
                return AwardedByUser.DisplayName;
            }
        }

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - AwardedAt;

                if (timeSpan.TotalSeconds < 60)
                    return $"{(int)timeSpan.TotalSeconds} seconds ago";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} days ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} months ago";

                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            }
        }
    }
}
