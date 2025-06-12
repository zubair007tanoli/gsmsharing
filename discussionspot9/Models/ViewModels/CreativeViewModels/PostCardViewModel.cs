using Microsoft.Identity.Client;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostCardViewModel
    {
        public int PostId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string PostType { get; set; } = "text";
        public string? Url { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public string PostUrl => $"/r/{CommunitySlug}/posts/{Slug}/test";
        public string AuthorUrl => $"/u/{AuthorDisplayName}";
        public bool IsSavedByUser { get; set; }
        public int Score => UpvoteCount - DownvoteCount;
        // Author
        public string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorInitials { get; set; } = string.Empty;

        // Community
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;

        // Tags
        public List<string> Tags { get; set; } = new();

        // Current user vote
        public int? CurrentUserVote { get; set; }

        // Calculated properties
      
        public string Excerpt => GetExcerpt();
        public string TimeAgo => GetTimeAgo(CreatedAt);
       
        public string CommunityUrl => $"/r/{CommunitySlug}";
        public string FormattedScore => FormatCount(Score);
        public string FormattedCommentCount => FormatCount(CommentCount);
        public string FormattedViewCount => FormatCount(ViewCount);
        public string? MediaUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? LinkDomain { get; set; }
    
        // Add alias for compatibility
        public string Username => AuthorDisplayName;

        // Add these for poll posts
        public int? PollVoteCount { get; set; }
        public DateTime? PollEndsAt { get; set; }

        // Add content preview property
        public string ContentPreview => GetContentPreview();

        private string GetContentPreview()
        {
            if (string.IsNullOrEmpty(Content))
                return string.Empty;

            var preview = Content.Length > 200 ? Content.Substring(0, 200) + "..." : Content;
            return preview;
        }
        public string VoteCountCssClass => Score switch
        {
            > 0 => "text-success",
            < 0 => "text-danger",
            _ => "text-muted"
        };

        private string GetExcerpt()
        {
            if (string.IsNullOrEmpty(Content)) return "";
            var text = System.Text.RegularExpressions.Regex.Replace(Content, "<.*?>", "");
            return text.Length > 150 ? text.Substring(0, 147) + "..." : text;
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();
            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 86400 => $"{(int)timeSpan.TotalHours}h ago",
                _ => $"{(int)timeSpan.TotalDays}d ago"
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
