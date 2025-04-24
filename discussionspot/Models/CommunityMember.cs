using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class CommunityMember
    {
        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int CommunityId { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "member";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string NotificationPreference { get; set; } = "all";

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        [ForeignKey(nameof(CommunityId))]
        public virtual Community Community { get; set; }
    }
}
