using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents an award that can be given to posts or comments
    /// </summary>
    public class Award : BaseEntity
    {
        [Key]
        public int AwardId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Award Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [StringLength(2048)]
        [Display(Name = "Icon URL")]
        [DataType(DataType.Url)]
        public string IconUrl { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Coin Cost")]
        public int CoinCost { get; set; }

        [Display(Name = "Giver Karma")]
        public int GiveKarma { get; set; } = 0;

        [Display(Name = "Receiver Karma")]
        public int ReceiveKarma { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<PostAward>? PostAwards { get; set; }
        public virtual ICollection<CommentAward>? CommentAwards { get; set; }

        // Helper methods
        /// <summary>
        /// Gets the total number of times this award has been given
        /// </summary>
        [NotMapped]
        public int TimesAwarded =>
            (PostAwards?.Count ?? 0) +
            (CommentAwards?.Count ?? 0);
    }
}
