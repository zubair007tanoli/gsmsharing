namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityCardViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public bool IsNSFW { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public DateTime? LastActivity { get; set; }
        public bool IsCurrentUserMember { get; set; }

        // Display properties
        public string CommunityUrl => $"/r/{Slug}";
        public string FormattedMemberCount => FormatCount(MemberCount);
        public string FormattedPostCount => FormatCount(PostCount);
        public string CategoryCssClass => CategorySlug?.ToLower().Replace("-", "") ?? "general";
        public string LastActivityText => GetLastActivityText();

        private static string FormatCount(int count)
        {
            return count switch
            {
                < 1000 => count.ToString(),
                < 10000 => $"{count / 1000.0:0.#}k",
                _ => $"{count / 1000}k"
            };
        }

        private string GetLastActivityText()
        {
            if (!LastActivity.HasValue) return "No activity yet";

            var timeSpan = DateTime.UtcNow - LastActivity.Value;
            return timeSpan.TotalMinutes switch
            {
                < 60 => "Active now",
                < 1440 => $"Active {(int)timeSpan.TotalHours}h ago",
                _ => $"Active {(int)timeSpan.TotalDays}d ago"
            };
        }
    }
}
