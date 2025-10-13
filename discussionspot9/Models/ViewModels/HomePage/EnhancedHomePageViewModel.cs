namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// Enhanced homepage with revenue optimization features
    /// </summary>
    public class EnhancedHomePageViewModel
    {
        // Core sections
        public List<TrendingPostViewModel> TrendingPosts { get; set; } = new();
        public List<FeaturedCommunityViewModel> FeaturedCommunities { get; set; } = new();
        public List<RecentPostViewModel> RecentPosts { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        
        // Engagement features
        public LiveActivityFeed ActivityFeed { get; set; } = new();
        public SiteStatsViewModel SiteStats { get; set; } = new();
        public List<TopContributorViewModel> TopContributors { get; set; } = new();
        
        // Revenue & Analytics
        public decimal TodayEarnings { get; set; }
        public int OnlineUsersCount { get; set; }
        public string HeroMessage { get; set; } = "Discover Discussions That Matter";
        public string HeroSubtext { get; set; } = "Join thousands of members sharing knowledge across technology, gaming, and more";
    }

    public class TrendingPostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsHot { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string AuthorDisplayName { get; set; } = string.Empty;
        public string TrendingBadge => IsHot ? "🔥 Hot" : CommentCount > 50 ? "💬 Active" : "⚡ Trending";
    }

    public class FeaturedCommunityViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsGrowing { get; set; }
        public string FormattedMembers => MemberCount >= 1000 ? $"{MemberCount / 1000.0:F1}K" : MemberCount.ToString();
    }

    public class LiveActivityFeed
    {
        public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
        public int TotalActivitiesLast24h { get; set; }
    }

    public class ActivityItemViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // replied, posted, upvoted
        public string TargetTitle { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Icon => ActionType.ToLower() switch
        {
            "replied" => "💬",
            "posted" => "✍️",
            "upvoted" => "⬆️",
            "joined" => "👋",
            _ => "•"
        };
    }

    public class SiteStatsViewModel
    {
        public int TotalMembers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int OnlineNow { get; set; }
        public int PostsToday { get; set; }
        public string FormattedMembers => TotalMembers >= 1000 ? $"{TotalMembers / 1000.0:F1}K+" : TotalMembers.ToString();
        public string FormattedPosts => TotalPosts >= 1000 ? $"{TotalPosts / 1000.0:F1}K+" : TotalPosts.ToString();
        public string FormattedComments => TotalComments >= 1000 ? $"{TotalComments / 1000.0:F1}K+" : TotalComments.ToString();
    }

    public class TopContributorViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int KarmaPoints { get; set; }
        public int PostsCount { get; set; }
        public bool IsVerified { get; set; }
    }
}

