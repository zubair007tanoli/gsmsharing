using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a user's vote on a comment
    /// </summary>
    public class CommentVote
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; } = string.Empty;

        [Key, Column(Order = 1)]
        public int CommentId { get; set; }

        [Required]
        [Display(Name = "Vote Type")]
        public int VoteType { get; set; }  // 1 for upvote, -1 for downvote

        [Display(Name = "Vote Date")]
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUsers User { get; set; } = null!;

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; } = null!;

        // Helper properties
        [NotMapped]
        public bool IsUpvote => VoteType == 1;

        [NotMapped]
        public bool IsDownvote => VoteType == -1;
    }
}
