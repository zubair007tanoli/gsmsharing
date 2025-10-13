namespace discussionspot9.Models.Configuration
{
    /// <summary>
    /// Configuration for Google Ads API (used for Keyword Planner)
    /// </summary>
    public class GoogleAdsConfiguration
    {
        public string DeveloperToken { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public bool IsConfigured { get; set; } = false;
        
        /// <summary>
        /// Default keywords to fetch if API fails
        /// </summary>
        public int MaxKeywordSuggestions { get; set; } = 50;
        public int MaxKeywordsPerPost { get; set; } = 15;
    }
}

