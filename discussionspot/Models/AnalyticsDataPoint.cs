namespace discussionspot.Models
{
    public class AnalyticsDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public DateTime? Date { get; set; }

        // Optional properties to allow for different types of analytics data
        public string? Category { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
    }
}
