namespace GsmsharingV2.Models.NewSchema
{
    /// <summary>
    /// Tracks affiliate link clicks with enhanced analytics
    /// </summary>
    public class AffiliateClick
    {
        public long ClickID { get; set; }
        public long? ProductID { get; set; }
        public long? UserId { get; set; } // Note: Changed from UserID to UserId for consistency
        
        // Tracking fields
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string ReferrerUrl { get; set; }
        public DateTime ClickDate { get; set; } = DateTime.UtcNow;
        
        // Conversion tracking
        public bool Converted { get; set; } = false; // If purchase was made
        public DateTime? ConversionDate { get; set; }
        public decimal? CommissionAmount { get; set; }

        // Navigation properties
        public AffiliateProductNew Product { get; set; }
    }
}

