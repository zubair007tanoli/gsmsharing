using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class PostAward
    {
        public int PostAwardId { get; set; }
        public int PostId { get; set; }
        public int AwardId { get; set; }
        public string? AwardedByUserId { get; set; }
        public DateTime AwardedAt { get; set; }
        public string? Message { get; set; }
        public bool IsAnonymous { get; set; }

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual Award Award { get; set; } = null!;
        public virtual IdentityUser? AwardedByUser { get; set; }
    }
}
