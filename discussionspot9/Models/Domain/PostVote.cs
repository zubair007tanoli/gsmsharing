using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class PostVote
    {
        public string UserId { get; set; } = null!;
        public int PostId { get; set; }
        public int VoteType { get; set; }
        public DateTime VotedAt { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
    }
}
