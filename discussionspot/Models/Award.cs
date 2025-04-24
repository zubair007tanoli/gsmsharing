using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Award
    {
        [Key]
        public int AwardId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(2048)]
        public string IconUrl { get; set; }

        public int CoinCost { get; set; }

        public int GiveKarma { get; set; } = 0;

        public int ReceiveKarma { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<PostAward> PostAwards { get; set; }
        public virtual ICollection<CommentAward> CommentAwards { get; set; }
    }
}
