// Models/ViewModels/HomePage/OnlineUserViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for individual online user
    /// </summary>
    public class OnlineUserViewModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the user
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Initials of the user for avatar display
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Initials { get; set; } = string.Empty;

        /// <summary>
        /// Current category the user is browsing
        /// </summary>
        [StringLength(100)]
        public string CurrentCategory { get; set; } = string.Empty;

        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Avatar URL if the user has one
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Whether the user is verified
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// User's role (if relevant to display)
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// Full URL to the user's profile
        /// </summary>
        public string ProfileUrl => $"/u/{DisplayName}";

        /// <summary>
        /// Whether the user has an avatar image
        /// </summary>
        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);

        /// <summary>
        /// Time since last activity
        /// </summary>
        public string LastSeenText => GetTimeAgo(LastActivity);

        /// <summary>
        /// Whether the user is currently active (within last 5 minutes)
        /// </summary>
        public bool IsCurrentlyActive => LastActivity > DateTime.UtcNow.AddMinutes(-5);

        /// <summary>
        /// Status indicator class for CSS styling
        /// </summary>
        public string StatusIndicatorClass => IsCurrentlyActive ? "online" : "away";

        /// <summary>
        /// Tooltip text for user hover
        /// </summary>
        public string TooltipText
        {
            get
            {
                var tooltip = $"{DisplayName}";

                if (!string.IsNullOrEmpty(CurrentCategory))
                    tooltip += $" - Active in {CurrentCategory}";

                if (!IsCurrentlyActive)
                    tooltip += $" - Last seen {LastSeenText}";

                return tooltip;
            }
        }

        /// <summary>
        /// CSS class for role-based styling
        /// </summary>
        public string RoleCssClass => Role.ToLower() switch
        {
            "admin" => "user-admin",
            "moderator" => "user-moderator",
            _ => "user-member"
        };

        /// <summary>
        /// Whether to show role badge
        /// </summary>
        public bool ShowRoleBadge => Role is "Admin" or "Moderator";

        /// <summary>
        /// Role badge icon
        /// </summary>
        public string RoleBadgeIcon => Role.ToLower() switch
        {
            "admin" => "fas fa-crown",
            "moderator" => "fas fa-shield-alt",
            _ => ""
        };

        /// <summary>
        /// Role badge color class
        /// </summary>
        public string RoleBadgeColorClass => Role.ToLower() switch
        {
            "admin" => "text-warning",
            "moderator" => "text-primary",
            _ => ""
        };

        /// <summary>
        /// Helper method to calculate time ago
        /// </summary>
        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 300 => "active", // Within 5 minutes
                < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 86400 => $"{(int)timeSpan.TotalHours}h ago",
                _ => dateTime.ToString("MMM dd")
            };
        }
    }
}