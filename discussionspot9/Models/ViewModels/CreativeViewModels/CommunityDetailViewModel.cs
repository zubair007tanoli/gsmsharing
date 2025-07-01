using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityDetailViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        [Required(ErrorMessage = "Community title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public string? BannerUrl { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Rules { get; set; }
        public bool IsNSFW { get; set; }
        public string CommunityType { get; set; } = "public";
        public string ThemeColor { get; set; }
        public string ShortDescription { get; set; }

        // Category info
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }

        // Creator info
        public string? CreatorId { get; set; }
        public string? CreatorDisplayName { get; set; }

        // Current user info
        public bool IsCurrentUserMember { get; set; }
        public bool IsCurrentUserModerator { get; set; }
        public string? CurrentUserRole { get; set; }

        // Moderators
        public List<ModeratorViewModel> Moderators { get; set; } = new();   

        public int? CategoryId { get; set; }
     
        // This will be set by the controller based on authenticated user
        public string? CurrentUserId { get; set; }
    
        // Formatted properties
        public string FormattedMemberCount => FormatCount(MemberCount);
        public string FormattedPostCount => FormatCount(PostCount);
        public string CreatedTimeAgo => GetTimeAgo(CreatedAt);

        private static string FormatCount(int count)
        {
            return count switch
            {
                < 1000 => count.ToString(),
                < 10000 => $"{count / 1000.0:0.#}k",
                < 1000000 => $"{count / 1000}k",
                _ => $"{count / 1000000.0:0.#}M"
            };
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();
            return timeSpan.TotalDays switch
            {
                < 1 => "today",
                < 30 => $"{(int)timeSpan.TotalDays} days ago",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)} months ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)} years ago"
            };
        }
    }
}
