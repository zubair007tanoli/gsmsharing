using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class CommentVote
    {
        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int CommentId { get; set; }

        public int VoteType { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        [ForeignKey(nameof(CommentId))]
        public virtual Comment Comment { get; set; }
    }
}
