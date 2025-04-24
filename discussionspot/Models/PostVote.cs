using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PostVote
    {
        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int PostId { get; set; }

        public int VoteType { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }
    }
}
