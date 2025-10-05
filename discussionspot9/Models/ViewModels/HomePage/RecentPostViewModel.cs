// Models/ViewModels/HomePage/RecentPostViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for recent posts displayed in the main content area
    /// </summary>
    public class RecentPostViewModel
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
        /// Full post content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Shortened excerpt of the post content
        /// </summary>
        [StringLength(300)]
        public string Excerpt { get; set; } = string.Empty;

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
        /// URL-friendly community identifier
        /// </summary>
        [Required]
        public string CommunitySlug { get; set; } = string.Empty;

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
        /// Net vote count (upvotes - downvotes)
        /// </summary>
        public int VoteCount { get; set; }

        /// <summary>
        /// Number of comments on this post
        /// </summary>
        [Range(0, int.MaxValue)]
        public int CommentCount { get; set; }

        /// <summary>
        /// Number of views this post has received
        /// </summary>
        [Range(0, int.MaxValue)]
        public int ViewCount { get; set; }

        /// <summary>
        /// When the post was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tags associated with this post
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Post type (text, link, image, video, poll)
        /// </summary>
        public string PostType { get; set; } = "text";

        /// <summary>
        /// Whether this post is pinned
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Whether this post is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Formatted time ago string (e.g., "2h ago")
        /// </summary>
        public string TimeAgo => GetTimeAgo(CreatedAt);

        /// <summary>
        /// Full URL to the post (community-based route)
        /// </summary>
        public string PostUrl => $"/r/{CommunitySlug}/posts/{Slug}";

        /// <summary>
        /// Full URL to the author's profile
        /// </summary>
        public string AuthorUrl => $"/u/{AuthorDisplayName}";

        /// <summary>
        /// CSS class for category styling
        /// </summary>
        public string CategoryCssClass => CategorySlug.ToLower().Replace("-", "");

        /// <summary>
        /// Formatted vote count for display
        /// </summary>
        public string FormattedVoteCount => FormatCount(VoteCount);

        /// <summary>
        /// Formatted comment count for display
        /// </summary>
        public string FormattedCommentCount => FormatCount(CommentCount);

        /// <summary>
        /// Formatted view count for display
        /// </summary>
        public string FormattedViewCount => FormatCount(ViewCount);

        /// <summary>
        /// Whether this post has any tags
        /// </summary>
        public bool HasTags => Tags.Any();

        /// <summary>
        /// Limited tags for display (max 3)
        /// </summary>
        public IEnumerable<string> DisplayTags => Tags.Take(3);

        /// <summary>
        /// Whether there are more tags than displayed
        /// </summary>
        public bool HasMoreTags => Tags.Count > 3;

        /// <summary>
        /// CSS class for vote count color
        /// </summary>
        public string VoteCountCssClass => VoteCount switch
        {
            > 0 => "text-success",
            < 0 => "text-danger",
            _ => "text-muted"
        };

        /// <summary>
        /// Icon class for post type
        /// </summary>
        public string PostTypeIcon => PostType.ToLower() switch
        {
            "link" => "fas fa-link",
            "image" => "fas fa-image",
            "video" => "fas fa-video",
            "poll" => "fas fa-poll",
            _ => "fas fa-file-text"
        };

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