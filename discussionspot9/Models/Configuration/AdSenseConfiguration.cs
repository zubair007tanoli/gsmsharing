namespace discussionspot9.Models.Configuration
{
    /// <summary>
    /// Configuration for Google AdSense API integration supporting multiple sites
    /// </summary>
    public class AdSenseConfiguration
    {
        public List<AdSenseSite> Sites { get; set; } = new();
        public string ServiceAccountEmail { get; set; } = string.Empty;
        public string ServiceAccountKeyPath { get; set; } = string.Empty;
        public bool UseServiceAccount { get; set; } = true;
        public int SyncIntervalHours { get; set; } = 24;
        public int HistoricalDataDays { get; set; } = 30;
    }

    public class AdSenseSite
    {
        public string Domain { get; set; } = string.Empty;
        public string AdClientId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string BaseUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// URL patterns to match posts from this site
        /// Example: ["discussionspot.com/r/", "discussionspot.com/post/"]
        /// </summary>
        public List<string> UrlPatterns { get; set; } = new();
    }
}

