namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostReportViewModel
    {
        public int ReportId { get; set; }
        public int PostId { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public string PostUrl { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByName { get; set; }
        public string? AdminNotes { get; set; }
        public string TimeAgo => GetTimeAgo(CreatedAt);
        
        private static string GetTimeAgo(DateTime dateTime)
        {
            var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            var span = DateTime.UtcNow - utcDateTime;
            
            if (span.TotalDays >= 365)
                return $"{(int)(span.TotalDays / 365)}y ago";
            if (span.TotalDays >= 30)
                return $"{(int)(span.TotalDays / 30)}mo ago";
            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays}d ago";
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours}h ago";
            if (span.TotalMinutes >= 1)
                return $"{(int)span.TotalMinutes}m ago";
            return "just now";
        }
    }
    
    public class SubmitReportRequest
    {
        public int PostId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}

