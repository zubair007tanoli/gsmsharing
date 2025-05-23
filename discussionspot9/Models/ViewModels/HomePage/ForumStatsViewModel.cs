// Models/ViewModels/HomePage/ForumStatsViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for forum statistics displayed in sidebar
    /// </summary>
    public class ForumStatsViewModel
    {
        /// <summary>
        /// Total number of registered members
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalMembers { get; set; }

        /// <summary>
        /// Total number of published posts
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalPosts { get; set; }

        /// <summary>
        /// Total number of active categories
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalCategories { get; set; }

        /// <summary>
        /// When the last post was created
        /// </summary>
        public DateTime LastPostTime { get; set; }

        /// <summary>
        /// Formatted "time ago" string for last post
        /// </summary>
        [StringLength(50)]
        public string LastPostTimeAgo { get; set; } = string.Empty;

        /// <summary>
        /// Total number of comments across all posts
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalComments { get; set; }

        /// <summary>
        /// Total number of communities
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalCommunities { get; set; }

        /// <summary>
        /// Forum establishment date
        /// </summary>
        public DateTime ForumCreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Formatted member count for display
        /// </summary>
        public string FormattedMemberCount => FormatCount(TotalMembers);

        /// <summary>
        /// Formatted post count for display
        /// </summary>
        public string FormattedPostCount => FormatCount(TotalPosts);

        /// <summary>
        /// Formatted comment count for display
        /// </summary>
        public string FormattedCommentCount => FormatCount(TotalComments);

        /// <summary>
        /// Formatted community count for display
        /// </summary>
        public string FormattedCommunityCount => FormatCount(TotalCommunities);

        /// <summary>
        /// Age of the forum in days
        /// </summary>
        public int ForumAgeInDays => (DateTime.UtcNow - ForumCreatedDate).Days;

        /// <summary>
        /// Whether the forum is newly created (less than 30 days)
        /// </summary>
        public bool IsNewForum => ForumAgeInDays < 30;

        /// <summary>
        /// Average posts per day since forum creation
        /// </summary>
        public double AveragePostsPerDay => ForumAgeInDays > 0 ? (double)TotalPosts / ForumAgeInDays : 0;

        /// <summary>
        /// Average posts per member
        /// </summary>
        public double AveragePostsPerMember => TotalMembers > 0 ? (double)TotalPosts / TotalMembers : 0;

        /// <summary>
        /// Whether the last post was recent (within 24 hours)
        /// </summary>
        public bool HasRecentActivity => LastPostTime > DateTime.UtcNow.AddHours(-24);

        /// <summary>
        /// Activity level description
        /// </summary>
        public string ActivityLevel
        {
            get
            {
                if (LastPostTime > DateTime.UtcNow.AddMinutes(-30))
                    return "Very Active";
                if (LastPostTime > DateTime.UtcNow.AddHours(-6))
                    return "Active";
                if (LastPostTime > DateTime.UtcNow.AddDays(-1))
                    return "Moderate";
                if (LastPostTime > DateTime.UtcNow.AddDays(-7))
                    return "Quiet";
                return "Slow";
            }
        }

        /// <summary>
        /// CSS class for activity level styling
        /// </summary>
        public string ActivityLevelCssClass => ActivityLevel.ToLower() switch
        {
            "very active" => "text-success",
            "active" => "text-success",
            "moderate" => "text-warning",
            "quiet" => "text-muted",
            "slow" => "text-muted",
            _ => "text-muted"
        };

        /// <summary>
        /// Growth rate description (based on recent activity)
        /// </summary>
        public string GrowthRate
        {
            get
            {
                var dailyAverage = AveragePostsPerDay;
                return dailyAverage switch
                {
                    >= 50 => "Rapidly Growing",
                    >= 20 => "Growing",
                    >= 5 => "Steady",
                    >= 1 => "Slow Growth",
                    _ => "Developing"
                };
            }
        }

        /// <summary>
        /// Statistics summary for display
        /// </summary>
        public List<StatisticItem> StatItems => new()
        {
            new StatisticItem("Members", FormattedMemberCount, "fas fa-users"),
            new StatisticItem("Posts", FormattedPostCount, "fas fa-comments"),
            new StatisticItem("Categories", TotalCategories.ToString(), "fas fa-folder"),
            new StatisticItem("Latest", LastPostTimeAgo, "fas fa-clock")
        };

        /// <summary>
        /// Whether the forum has enough content to show meaningful stats
        /// </summary>
        public bool HasMeaningfulStats => TotalMembers > 0 && TotalPosts > 0;

        /// <summary>
        /// Helper method to format large numbers
        /// </summary>
        private static string FormatCount(int count)
        {
            return count switch
            {
                < 1000 => count.ToString("N0"),
                < 10000 => $"{count / 1000.0:0.#}k",
                < 1000000 => $"{count / 1000:N0}k",
                < 10000000 => $"{count / 1000000.0:0.#}M",
                _ => $"{count / 1000000:N0}M"
            };
        }
    }

    /// <summary>
    /// Helper class for statistic display items
    /// </summary>
    public class StatisticItem
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;

        public StatisticItem() { }

        public StatisticItem(string label, string value, string iconClass)
        {
            Label = label;
            Value = value;
            IconClass = iconClass;
        }
    }
}