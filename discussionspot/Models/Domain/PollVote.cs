using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a user's vote on a poll option
    /// </summary>
    public class PollVote
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; } = string.Empty;

        [Key, Column(Order = 1)]
        public int PollOptionId { get; set; }

        [Display(Name = "Vote Date")]
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUsers User { get; set; } = null!;

        [ForeignKey("PollOptionId")]
        public virtual PollOption PollOption { get; set; } = null!;
    }
}
