using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    public class NewMemberViewModel
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
        /// When the user joined the forum
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// Formatted "time ago" string for join date
        /// </summary>
        [StringLength(50)]
        public string JoinDateAgo { get; set; } = string.Empty;

        /// <summary>
        /// Avatar URL if the user has one
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Whether the user is verified
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// User's bio/description
        /// </summary>
        [StringLength(500)]
        public string? Bio { get; set; }

        /// <summary>
        /// Number of posts made by this user
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PostCount { get; set; }

        /// <summary>
        /// User's karma points
        /// </summary>
        public int KarmaPoints { get; set; }

        /// <summary>
        /// Full URL to the user's profile
        /// </summary>
        public string ProfileUrl => $"/u/{DisplayName}";

        /// <summary>
        /// Whether the user has an avatar image
        /// </summary>
        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);

        /// <summary>
        /// Whether the user joined today
        /// </summary>
        public bool JoinedToday => JoinDate.Date == DateTime.Today;

        /// <summary>
        /// Whether the user joined this week
        /// </summary>
        public bool JoinedThisWeek => JoinDate > DateTime.UtcNow.AddDays(-7);

        /// <summary>
        /// Whether the user is brand new (joined within 24 hours)
        /// </summary>
        public bool IsBrandNew => JoinDate > DateTime.UtcNow.AddHours(-24);

        /// <summary>
        /// CSS class for new member styling
        /// </summary>
        public string NewMemberCssClass => IsBrandNew ? "brand-new-member" : "new-member";

        /// <summary>
        /// Badge text for new member
        /// </summary>
        public string BadgeText
        {
            get
            {
                if (JoinedToday) return "Joined today";
                if (JoinedThisWeek) return "New this week";
                return "New member";
            }
        }

        /// <summary>
        /// Badge CSS class
        /// </summary>
        public string BadgeCssClass => JoinedToday ? "badge bg-success" : "badge bg-primary";

        /// <summary>
        /// Whether the user has made any posts
        /// </summary>
        public bool HasPosts => PostCount > 0;

        /// <summary>
        /// Activity level description
        /// </summary>
        public string ActivityLevel => PostCount switch
        {
            0 => "Lurker",
            1 => "First post",
            <= 5 => "Getting started",
            <= 20 => "Active",
            _ => "Very active"
        };

        /// <summary>
        /// Welcome message for this member
        /// </summary>
        public string WelcomeMessage
        {
            get
            {
                if (JoinedToday)
                    return $"Welcome {DisplayName}!";
                if (JoinedThisWeek)
                    return $"New member: {DisplayName}";
                return DisplayName;
            }
        }

        /// <summary>
        /// Tooltip text for member hover
        /// </summary>
        public string TooltipText
        {
            get
            {
                var tooltip = $"{DisplayName} - Joined {JoinDateAgo}";

                if (HasPosts)
                    tooltip += $" - {PostCount} post{(PostCount != 1 ? "s" : "")}";

                if (KarmaPoints > 0)
                    tooltip += $" - {KarmaPoints} karma";

                return tooltip;
            }
        }

        /// <summary>
        /// Member stats for display
        /// </summary>
        public string StatsText
        {
            get
            {
                var stats = new List<string>();

                if (HasPosts)
                    stats.Add($"{PostCount} post{(PostCount != 1 ? "s" : "")}");

                if (KarmaPoints > 0)
                    stats.Add($"{KarmaPoints} karma");

                return stats.Any() ? string.Join(" • ", stats) : "No activity yet";
            }
        }

        /// <summary>
        /// Whether to show detailed stats
        /// </summary>
        public bool ShowDetailedStats => HasPosts || KarmaPoints > 0;

        /// <summary>
        /// Short bio for display (truncated)
        /// </summary>
        public string? ShortBio => !string.IsNullOrEmpty(Bio) && Bio.Length > 50
            ? $"{Bio.Substring(0, 47)}..."
            : Bio;

        /// <summary>
        /// Whether the user has a bio
        /// </summary>
        public bool HasBio => !string.IsNullOrEmpty(Bio);
    }
}
