using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class CommentAward
    {
        public int CommentAwardId { get; set; }
        public int CommentId { get; set; }
        public int AwardId { get; set; }
        public string? AwardedByUserId { get; set; }
        public DateTime AwardedAt { get; set; }
        public string? Message { get; set; }
        public bool IsAnonymous { get; set; }

        // Navigation properties
        public virtual Comment Comment { get; set; } = null!;
        public virtual Award Award { get; set; } = null!;
        public virtual IdentityUser? AwardedByUser { get; set; }
    }
}
