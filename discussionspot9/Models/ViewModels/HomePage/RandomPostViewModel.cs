// Models/ViewModels/HomePage/RandomPostViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for random posts displayed in the featured section
    /// </summary>
    public class RandomPostViewModel
    {
        /// <summary>
        /// Unique identifier for the post
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// Post title
        /// </summary>
        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// URL-friendly version of the title
        /// </summary>
        [Required]
        [StringLength(320)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Name of the category this post belongs to
        /// </summary>
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// URL-friendly category identifier
        /// </summary>
        [Required]
        public string CategorySlug { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the post author
        /// </summary>
        [Required]
        public string AuthorDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Initials of the author for avatar display
        /// </summary>
        [Required]
        [StringLength(3)]
        public string AuthorInitials { get; set; } = string.Empty;

        /// <summary>
        /// Number of comments on this post
        /// </summary>
        [Range(0, int.MaxValue)]
        public int CommentCount { get; set; }

        /// <summary>
        /// When the post was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Formatted time ago string (e.g., "2h ago")
        /// </summary>
        public string TimeAgo => GetTimeAgo(CreatedAt);

        /// <summary>
        /// Full URL to the post
        /// </summary>
        public string PostUrl => $"/r/{CategorySlug}/posts/{Slug}";

        /// <summary>
        /// CSS class for category styling
        /// </summary>
        public string CategoryCssClass => CategorySlug.ToLower().Replace("-", "");

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