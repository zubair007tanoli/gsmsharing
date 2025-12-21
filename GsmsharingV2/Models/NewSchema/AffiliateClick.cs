namespace GsmsharingV2.Models.NewSchema
{
    public class AffiliateClick
    {
        public long ClickID { get; set; }
        public long ProductID { get; set; }
        public long? UserID { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime ClickDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AffiliateProductNew Product { get; set; }
    }
}

