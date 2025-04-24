using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class CommunityMemberViewModel
    {
        /// <summary>
        /// The ID of the user who is a member of the community
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The display name of the user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The user's avatar URL
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// The ID of the community the user belongs to
        /// </summary>
        public int CommunityId { get; set; }

        /// <summary>
        /// The name of the community
        /// </summary>
        public string CommunityName { get; set; }

        /// <summary>
        /// The role of the user in the community (member, moderator, admin)
        /// </summary>
        [Required]
        public string Role { get; set; }

        /// <summary>
        /// The date when the user joined the community
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// The notification preference for this community membership
        /// </summary>
        [Required]
        public string NotificationPreference { get; set; }

        /// <summary>
        /// The karma points of the user
        /// </summary>
        public int KarmaPoints { get; set; }

        /// <summary>
        /// Flag indicating if the user is verified
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Date when the user's account was created
        /// </summary>
        public DateTime UserJoinDate { get; set; }

        /// <summary>
        /// Date when the user was last active
        /// </summary>
        public DateTime LastActive { get; set; }
    }
}
