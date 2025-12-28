namespace GsmsharingV2.Models.NewSchema
{
    /// <summary>
    /// Affiliate product with enhanced fields for Amazon and AliExpress
    /// </summary>
    public class AffiliateProductNew
    {
        public long ProductID { get; set; }
        public int? PartnerID { get; set; }

        public string Title { get; set; }
        public string Category { get; set; }

        // Links
        public string OriginalLink { get; set; }
        public string AffiliateLink { get; set; }

        // Media
        public string ImageUrl { get; set; }

        // Pricing
        public decimal? PriceDisplay { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string Currency { get; set; } = "USD";

        // Amazon-specific fields
        public string ASIN { get; set; } // Amazon Standard Identification Number
        public decimal? Rating { get; set; } // Product rating (1-5 stars)
        public int? ReviewCount { get; set; }
        public string Availability { get; set; }
        public bool? PrimeEligible { get; set; }
        public bool? BestSeller { get; set; }
        public bool? AmazonChoice { get; set; }
        public string ProductCategory { get; set; }
        public string Brand { get; set; }

        // AliExpress-specific fields
        public string AliExpressProductId { get; set; }

        // Stats
        public int Clicks { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AffiliatePartner Partner { get; set; }
        public ICollection<AffiliateClick> ClicksData { get; set; } = new List<AffiliateClick>();
    }
}

