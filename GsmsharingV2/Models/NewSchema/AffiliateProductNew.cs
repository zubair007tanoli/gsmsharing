namespace GsmsharingV2.Models.NewSchema
{
    public class AffiliateProductNew
    {
        public long ProductID { get; set; }
        public int? PartnerID { get; set; }

        public string Title { get; set; }
        public string Category { get; set; }

        public string OriginalLink { get; set; }
        public string AffiliateLink { get; set; }

        public string ImageUrl { get; set; }
        public decimal? PriceDisplay { get; set; }

        public int Clicks { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AffiliatePartner Partner { get; set; }
        public ICollection<AffiliateClick> ClicksData { get; set; } = new List<AffiliateClick>();
    }
}

