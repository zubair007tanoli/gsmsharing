namespace GsmsharingV2.Models.NewSchema
{
    /// <summary>
    /// Affiliate marketing partner (Amazon, AliExpress, etc.)
    /// </summary>
    public class AffiliatePartner
    {
        public int PartnerID { get; set; }
        public string Name { get; set; }
        public string PartnerType { get; set; } = "amazon"; // 'amazon', 'aliexpress', 'other'
        public string AffiliateTag { get; set; } // Legacy field
        public string BaseUrl { get; set; }
        
        // Enhanced fields for API integration
        public string TrackingId { get; set; } // Amazon Associate Tag / AliExpress PID
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public decimal? CommissionRate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<AffiliateProductNew> Products { get; set; } = new List<AffiliateProductNew>();
    }
}

