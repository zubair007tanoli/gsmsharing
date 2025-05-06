using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a user's membership in a community with role and preferences
    /// </summary>
    public class CommunityMember
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; } = string.Empty;

        [Key, Column(Order = 1)]
        public int CommunityId { get; set; }

        [StringLength(20)]
        [Display(Name = "Role")]
        public string Role { get; set; } = "member";  // member, moderator, admin

        [Display(Name = "Joined Date")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        [Display(Name = "Notification Preference")]
        public string NotificationPreference { get; set; } = "all";  // all, important, none

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUsers User { get; set; } = null!;

        [ForeignKey("CommunityId")]
        public virtual Community Community { get; set; } = null!;

        // Helper properties
        [NotMapped]
        public bool IsModerator => Role == "moderator" || Role == "admin";

        [NotMapped]
        public bool IsAdmin => Role == "admin";
    }
}
