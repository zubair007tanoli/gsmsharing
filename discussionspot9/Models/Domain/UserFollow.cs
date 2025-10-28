using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Represents a follower relationship between two users
    /// </summary>
    public class UserFollow
    {
        /// <summary>
        /// Primary key for the follow relationship
        /// </summary>
        [Key]
        public int FollowId { get; set; }

        /// <summary>
        /// ID of the user who is following (the follower)
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string FollowerId { get; set; } = null!;

        /// <summary>
        /// ID of the user being followed
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string FollowedId { get; set; } = null!;

        /// <summary>
        /// When the follow relationship was created
        /// </summary>
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the follower receives notifications about the followed user's activity
        /// </summary>
        public bool NotificationsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the follow relationship is still active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// The user who is following
        /// </summary>
        public virtual IdentityUser? Follower { get; set; }

        /// <summary>
        /// The user being followed
        /// </summary>
        public virtual IdentityUser? Followed { get; set; }
    }
}

