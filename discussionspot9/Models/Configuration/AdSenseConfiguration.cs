namespace discussionspot9.Models.Configuration
{
    public class AdSenseConfiguration
    {
        public string ClientId { get; set; } = string.Empty;
        public string PublisherId { get; set; } = string.Empty;
        public string AdSlotId { get; set; } = string.Empty;
        public string StoryAdSlotId { get; set; } = string.Empty;
        public string AmpAdSlotId { get; set; } = string.Empty;
        public bool EnableAds { get; set; } = true;
        public bool EnableStoryAds { get; set; } = true;
        public bool EnableAmpAds { get; set; } = true;
        public int AdFrequency { get; set; } = 3; // Show ad every 3 slides
        public List<AdSenseSite> Sites { get; set; } = new List<AdSenseSite>();
        public bool UseServiceAccount { get; set; } = false;
        public string ServiceAccountEmail { get; set; } = string.Empty;
        public string ServiceAccountKeyPath { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string OAuthClientId { get; set; } = string.Empty;
        public string OAuthClientSecret { get; set; } = string.Empty;
        public string OAuthRefreshToken { get; set; } = string.Empty;
        public string OAuthUserEmail { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = "DiscussionSpot AdSense Sync";
        public int HistoricalDataDays { get; set; } = 30;
        public int SyncIntervalHours { get; set; } = 24;
    }
}