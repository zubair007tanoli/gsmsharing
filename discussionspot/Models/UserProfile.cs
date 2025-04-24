using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot.Models
{
    public class UserProfile
    {
        [Key]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        public string Bio { get; set; }

        [StringLength(2048)]
        public string AvatarUrl { get; set; }

        [StringLength(2048)]
        public string BannerUrl { get; set; }

        [StringLength(2048)]
        public string Website { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        public int KarmaPoints { get; set; } = 0;

        public bool IsVerified { get; set; } = false;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        // Navigation properties
        public virtual ICollection<CommunityMember> CommunityMemberships { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
