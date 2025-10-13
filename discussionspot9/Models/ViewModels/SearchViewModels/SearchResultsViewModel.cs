namespace discussionspot9.Models.ViewModels.SearchViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public string CurrentType { get; set; } = "all";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public List<SearchPostResult> Posts { get; set; } = new();
        public List<SearchCommunityResult> Communities { get; set; } = new();
        public List<SearchUserResult> Users { get; set; } = new();

        public int TotalPosts { get; set; }
        public int TotalCommunities { get; set; }
        public int TotalUsers { get; set; }
        public int TotalResults => TotalPosts + TotalCommunities + TotalUsers;

        public bool HasResults => TotalResults > 0;
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
        public int Score { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
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

