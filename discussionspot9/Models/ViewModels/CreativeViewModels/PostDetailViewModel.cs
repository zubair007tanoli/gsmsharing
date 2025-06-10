using discussionspot9.Models.ViewModels.PollViewModels;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostDetailViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string PostType { get; set; } = "text";
        public string? Url { get; set; }
        public string? LinkPreviewImage { get; set; }
        public string? LinkPreviewDescription { get; set; }
        public string? LinkDomain { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public bool HasPoll { get; set; }
        public bool IsSavedByUser { get; set; }

        // Author info
        public string? UserId { get; set; }
        public string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorInitials { get; set; } = string.Empty;
        public int AuthorKarma { get; set; }

        // Community info
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string? CommunityIconUrl { get; set; }

        // Tags
        public List<string> Tags { get; set; } = new();

        // Media
        public List<MediaViewModel> Media { get; set; } = new();

        // Poll (if applicable)
        //public PollViewModel? Poll { get; set; }
        public PollViewModel Poll { get; set; } = new();

        // Awards
        public List<PostAwardViewModel> Awards { get; set; } = new();

        // Current user interaction
        public int? CurrentUserVote { get; set; }
        public bool IsCurrentUserAuthor { get; set; }

        public LinkPreviewViewModel LinkModel = new();
        // Calculated properties
        public int Score => UpvoteCount - DownvoteCount;
        public string TimeAgo => GetTimeAgo(CreatedAt);
        public string PostUrl => $"/r/{CommunitySlug}/posts/{Slug}";
        public string AuthorUrl => $"/u/{AuthorDisplayName}";
        public string FormattedViewCount => FormatCount(ViewCount);
        public string FormattedCommentCount => FormatCount(CommentCount);

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();
            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 86400 => $"{(int)timeSpan.TotalHours}h ago",
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }

        private static string FormatCount(int count)
        {
            return count switch
            {
                < 1000 => count.ToString(),
                < 10000 => $"{count / 1000.0:0.#}k",
                _ => $"{count / 1000}k"
            };
        }
    }
}
