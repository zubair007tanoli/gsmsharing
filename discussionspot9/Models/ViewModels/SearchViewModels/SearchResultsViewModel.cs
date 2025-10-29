namespace discussionspot9.Models.ViewModels.SearchViewModels
{
    public class SearchResultsViewModel
    {
        // Basic search properties
        public string Query { get; set; } = string.Empty;
        public string CurrentType { get; set; } = "all";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        // NEW: Filter properties
        public string SortBy { get; set; } = "relevance"; // relevance, new, hot, top
        public string PostType { get; set; } = "all"; // all, text, link, image, video, poll
        public bool HasMedia { get; set; } = false;
        public string TimeRange { get; set; } = "all"; // hour, day, week, month, year, all
        public int MinKarma { get; set; } = 0;
        public bool VerifiedOnly { get; set; } = false;
        public int MinScore { get; set; } = 0;

        // Results
        public List<SearchPostResult> Posts { get; set; } = new();
        public List<SearchCommunityResult> Communities { get; set; } = new();
        public List<SearchUserResult> Users { get; set; } = new();

        // Counts
        public int TotalPosts { get; set; }
        public int TotalCommunities { get; set; }
        public int TotalUsers { get; set; }
        public int TotalResults => TotalPosts + TotalCommunities + TotalUsers;

        // Helpers
        public bool HasResults => TotalResults > 0;
        public bool HasActiveFilters => SortBy != "relevance" || PostType != "all" || 
                                        HasMedia || TimeRange != "all" || 
                                        MinKarma > 0 || VerifiedOnly || MinScore > 0;
    }

    public class SearchPostResult
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int AuthorKarma { get; set; }
        public int Score { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PostType { get; set; } = "text";
        public string? ThumbnailUrl { get; set; } // For link previews or media
        public string? LinkPreviewDomain { get; set; }
        public string TimeAgo => FormatTimeAgo(DateTime.UtcNow - CreatedAt);
        public string Url => $"/r/{CommunitySlug}/posts/{Slug}";

        private static string FormatTimeAgo(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        }
    }

    public class SearchCommunityResult
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public string? IconUrl { get; set; }
        public string Url => $"/r/{Slug}";
    }

    public class SearchUserResult
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        public string Url => $"/u/{DisplayName}";
    }
}

