using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class PollVote
    {
        public string UserId { get; set; } = null!;
        public int PollOptionId { get; set; }
        public DateTime VotedAt { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual PollOption PollOption { get; set; } = null!;
    }
}
