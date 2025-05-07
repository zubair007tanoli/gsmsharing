namespace discussionspot.ViewModels
{
    public class PostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string PostType { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int CommentCount { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public int ViewCount { get; set; }
        public bool HasPoll { get; set; }
        // Add to PostViewModel.cs

        // User interaction properties
        public bool? UserVoted { get; set; } // null if not voted, true for upvote, false for downvote
        public bool UserSaved { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanModerate { get; set; }

        // Community details
        public string CommunityIconUrl { get; set; }
        public string CommunitySlug { get; set; }
        public string CommunityType { get; set; }

        // User details
        public string UserAvatarUrl { get; set; }
        public DateTime UserJoinDate { get; set; }

        // Poll configuration
        public PollConfigurationViewModel PollConfig { get; set; }

        // Helper for displaying relative time
        public string RelativeTime
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;

                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays}d ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)}mo ago";

                return $"{(int)(timeSpan.TotalDays / 365)}y ago";
            }
        }

        // For link posts - extract domain name
        public string LinkDomain
        {
            get
            {
                if (string.IsNullOrEmpty(Url) || !Uri.TryCreate(Url, UriKind.Absolute, out Uri uri))
                    return string.Empty;

                return uri.Host.Replace("www.", "");
            }
        }
        public IEnumerable<PollOptionViewModel> PollOptions { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<MediaViewModel> Media { get; set; }
    }
}
