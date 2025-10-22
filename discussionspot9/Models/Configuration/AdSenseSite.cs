namespace discussionspot9.Models.Configuration
{
    public class AdSenseSite
    {
        public string Domain { get; set; } = string.Empty;
        public string AdClientId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string BaseUrl { get; set; } = string.Empty;
        public List<string> UrlPatterns { get; set; } = new List<string>();
    }
}
