using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents an option in a poll
    /// </summary>
    public class PollOption : BaseEntity
    {
        [Key]
        public int PollOptionId { get; set; }

        [Required]
        [Display(Name = "Post")]
        public int PostId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Option Text")]
        public string OptionText { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Vote Count")]
        public int VoteCount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        public virtual ICollection<PollVote>? Votes { get; set; }

        // Helper properties
        /// <summary>
        /// Gets the percentage of total votes for this option
        /// </summary>
        [NotMapped]
        public double VotePercentage
        {
            get
            {
                if (Post.PollVoteCount == 0) return 0;
                return Math.Round((double)VoteCount / Post.PollVoteCount * 100, 1);
            }
        }

        /// <summary>
        /// Gets a formatted string of the vote percentage
        /// </summary>
        [NotMapped]
        public string VotePercentageFormatted => $"{VotePercentage}%";
    }
}
