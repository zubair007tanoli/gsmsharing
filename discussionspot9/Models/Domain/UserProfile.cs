using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class UserProfile
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        
        /// <summary>
        /// If true, user is currently site-banned
        /// </summary>
        public bool IsBanned { get; set; }
        
        /// <summary>
        /// Ban expiration date (null = permanent ban)
        /// </summary>
        public DateTime? BanExpiresAt { get; set; }
        
        /// <summary>
        /// Reason for ban (shown to user)
        /// </summary>
        public string? BanReason { get; set; }

        // Navigation property
        public virtual IdentityUser User { get; set; } = null!;
    }
}
