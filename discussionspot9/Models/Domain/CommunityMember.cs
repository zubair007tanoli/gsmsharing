using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class CommunityMember
    {
        public string UserId { get; set; } = null!;
        public int CommunityId { get; set; }
        public string Role { get; set; } = "member";
        public DateTime JoinedAt { get; set; }
        public string NotificationPreference { get; set; } = "all";

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual Community Community { get; set; } = null!;
    }
}
