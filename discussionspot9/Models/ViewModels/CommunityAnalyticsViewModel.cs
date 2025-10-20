using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class CommunityAnalyticsViewModel
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        
        // Date range
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string DateRange { get; set; } = "30d";
        
        // Overview metrics
        public int TotalMembers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalViews { get; set; }
        public double EngagementRate { get; set; }
        public double GrowthRate { get; set; }
        
        // Time series data
        public List<AnalyticsDataPoint> MemberGrowth { get; set; } = new();
        public List<AnalyticsDataPoint> PostActivity { get; set; } = new();
        public List<AnalyticsDataPoint> CommentActivity { get; set; } = new();
        public List<AnalyticsDataPoint> ViewActivity { get; set; } = new();
        
        // Top contributors
        public List<TopContributorViewModel> TopContributors { get; set; } = new();
        public List<TopContributorViewModel> TopPosters { get; set; } = new();
        public List<TopContributorViewModel> TopCommenters { get; set; } = new();
        
        // Activity heatmap
        public List<ActivityHeatmapData> ActivityHeatmap { get; set; } = new();
        
        // Traffic sources
        public List<TrafficSourceViewModel> TrafficSources { get; set; } = new();
        
        // Export options
        public bool CanExportData { get; set; }
    }
    
    public class AnalyticsDataPoint
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
    }
    
    public class TopContributorViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Initials { get; set; } = string.Empty;
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int TotalKarma { get; set; }
        public int Rank { get; set; }
        public string Role { get; set; } = string.Empty;
    }
    
    public class ActivityHeatmapData
    {
        public int Hour { get; set; }
        public int DayOfWeek { get; set; }
        public int ActivityCount { get; set; }
        public string DayName { get; set; } = string.Empty;
    }
    
    public class TrafficSourceViewModel
    {
        public string Source { get; set; } = string.Empty;
        public int Visitors { get; set; }
        public double Percentage { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
