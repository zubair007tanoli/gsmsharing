// Models/ViewModels/HomePage/TrendingTopicViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for trending topics displayed in the sidebar
    /// </summary>
    public class TrendingTopicViewModel
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
        /// Community slug for post URL
        /// </summary>
        [Required]
        public string CommunitySlug { get; set; } = string.Empty;

        /// <summary>
        /// Number of replies/comments on this post
        /// </summary>
        [Range(0, int.MaxValue)]
        public int ReplyCount { get; set; }

        /// <summary>
        /// Whether this topic is marked as "hot" (recent high activity)
        /// </summary>
        public bool IsHot { get; set; }

        /// <summary>
        /// Trending score (calculated based on votes, comments, views, recency)
        /// </summary>
        public int TrendingScore { get; set; }

        /// <summary>
        /// Upvote count for display
        /// </summary>
        public int UpvoteCount { get; set; }

        /// <summary>
        /// Downvote count for display
        /// </summary>
        public int DownvoteCount { get; set; }

        /// <summary>
        /// When the post was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last activity on this post (last comment, vote, etc.)
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Full URL to the post
        /// </summary>
        public string PostUrl => $"/r/{CommunitySlug}/posts/{Slug}";

        /// <summary>
        /// CSS class for category styling
        /// </summary>
        public string CategoryCssClass => CategorySlug.ToLower().Replace("-", "");

        /// <summary>
        /// Formatted reply count for display
        /// </summary>
        public string FormattedReplyCount => FormatCount(ReplyCount);

        /// <summary>
        /// Shortened title for sidebar display (max 60 characters)
        /// </summary>
        public string ShortTitle => Title.Length > 60 ? $"{Title.Substring(0, 57)}..." : Title;

        /// <summary>
        /// CSS class for hot topic styling
        /// </summary>
        public string HotTopicCssClass => IsHot ? "hot-topic" : "";

        /// <summary>
        /// Status text for the topic
        /// </summary>
        public string StatusText => IsHot ? "Hot" : "";

        /// <summary>
        /// Whether this topic was created recently (within 24 hours)
        /// </summary>
        public bool IsRecent => CreatedAt > DateTime.UtcNow.AddHours(-24);

        /// <summary>
        /// Whether this topic has recent activity (within 6 hours)
        /// </summary>
        public bool HasRecentActivity => LastActivity > DateTime.UtcNow.AddHours(-6);

        /// <summary>
        /// Time since last activity
        /// </summary>
        public string LastActivityText => GetTimeAgo(LastActivity);

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