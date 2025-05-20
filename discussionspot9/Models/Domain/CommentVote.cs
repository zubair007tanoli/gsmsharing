using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class CommentVote
    {
        public string UserId { get; set; } = null!;
        public int CommentId { get; set; }
        public int VoteType { get; set; }
        public DateTime VotedAt { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual Comment Comment { get; set; } = null!;
    }
}
