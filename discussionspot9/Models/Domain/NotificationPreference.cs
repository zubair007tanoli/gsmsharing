using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// User notification preferences - controls what notifications they receive and how
    /// </summary>
    public class NotificationPreference
    {
        [Key]
        public int PreferenceId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = null!; // comment, reply, vote, follow, mention, etc.

        public bool WebEnabled { get; set; } = true;

        public bool EmailEnabled { get; set; } = true;

        public bool PushEnabled { get; set; } = false;

        [MaxLength(20)]
        public string EmailFrequency { get; set; } = "instant"; // instant, daily, weekly, never

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual IdentityUser User { get; set; } = null!;
    }

    /// <summary>
    /// User-level notification settings (global preferences)
    /// </summary>
    public class UserNotificationSettings
    {
        [Key]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        // Global toggles
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool WebNotificationsEnabled { get; set; } = true;
        public bool PushNotificationsEnabled { get; set; } = false;

        // Email digest
        public string EmailDigestFrequency { get; set; } = "never"; // never, daily, weekly

        // Quiet hours
        public bool QuietHoursEnabled { get; set; } = false;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }

        // Preferences
        public bool GroupNotifications { get; set; } = true; // "John and 5 others..."
        public bool ShowNotificationPreviews { get; set; } = true;
        public bool PlayNotificationSound { get; set; } = false;

        // Email settings
        public bool UnsubscribeFromAll { get; set; } = false;
        public DateTime? LastDigestSent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual IdentityUser User { get; set; } = null!;
    }
}

