// Models/ViewModels/HomePage/CategoryViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for category display with statistics
    /// </summary>
    public class CategoryViewModel
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL-friendly category identifier
        /// </summary>
        [Required]
        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Category description
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// FontAwesome icon class for this category
        /// </summary>
        [Required]
        public string IconClass { get; set; } = "fas fa-folder";

        /// <summary>
        /// Number of topics/communities in this category
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TopicCount { get; set; }

        /// <summary>
        /// Total number of posts in this category
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PostCount { get; set; }

        /// <summary>
        /// Whether there's current activity in this category
        /// </summary>
        public bool IsActiveNow { get; set; }

        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime? LastActivity { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Whether this category is active/visible
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Full URL to the category
        /// </summary>
        public string CategoryUrl => $"/r/{Slug}";

        /// <summary>
        /// CSS class for category styling
        /// </summary>
        public string CategoryCssClass => Slug.ToLower().Replace("-", "");

        /// <summary>
        /// Formatted last activity string
        /// </summary>
        public string LastActivityText
        {
            get
            {
                if (!LastActivity.HasValue)
                    return "No activity yet";

                if (IsActiveNow)
                    return "Active now";

                return GetTimeAgo(LastActivity.Value);
            }
        }

        /// <summary>
        /// Formatted post count for display
        /// </summary>
        public string FormattedPostCount => FormatCount(PostCount);

        /// <summary>
        /// Formatted topic count for display
        /// </summary>
        public string FormattedTopicCount => FormatCount(TopicCount);

        /// <summary>
        /// Helper method to format large numbers
        /// </summary>
        private static string FormatCount(int count)
        {
            return count switch
            {
                < 1000 => count.ToString(),
                < 10000 => $"{count / 1000.0:0.#}k",
                < 1000000 => $"{count / 1000}k",
                < 10000000 => $"{count / 1000000.0:0.#}M",
                _ => $"{count / 1000000}M"
            };
        }

        /// <summary>
        /// Helper method to calculate time ago
        /// </summary>
        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 86400 => $"{(int)timeSpan.TotalHours}h ago",
                < 2592000 => $"{(int)timeSpan.TotalDays}d ago",
                _ => dateTime.ToString("MMM dd")
            };
        }
    }
}