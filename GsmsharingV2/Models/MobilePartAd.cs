namespace GsmsharingV2.Models
{
    /// <summary>
    /// Represents a mobile part advertisement with comprehensive details
    /// </summary>
    public class MobilePartAd
    {
        public int MobileAdsId { get; set; }
        public string UserId { get; set; }
        
        // Basic Information
        public string Title { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        
        // Part Details
        public string PartType { get; set; } // Screen, Battery, Camera, Motherboard, etc.
        public string PartCondition { get; set; } // New, Used, Refurbished
        public string BrandCompatibility { get; set; } // Samsung, Apple, etc.
        public string ModelCompatibility { get; set; } // iPhone 13, Galaxy S21, etc.
        public string PartNumber { get; set; }
        public string SKU { get; set; }
        
        // Pricing
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsNegotiable { get; set; }
        public decimal? OriginalPrice { get; set; }
        
        // Condition Details
        public string QualityGrade { get; set; } // OEM, aftermarket, Original Refurbished
        public string ConditionNotes { get; set; }
        public bool HasWarranty { get; set; }
        public int? WarrantyMonths { get; set; }
        
        // Location
        public string Location { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool OffersShipping { get; set; }
        public decimal? ShippingCost { get; set; }
        public string ShippingMethods { get; set; }
        
        // Media
        public string FeaturedImage { get; set; }
        public ICollection<AdImage> Images { get; set; }
        
        // Status
        public string AdStatus { get; set; } // Active, Sold, Expired, Pending, Rejected
        public byte? Publish { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPromoted { get; set; }
        
        // SEO
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        
        // Tracking
        public int? Views { get; set; } = 0
;
        public int? Likes { get; set; } = 0;
        public int? Favorites { get; set; } = 0;
        
        // Timestamps
        public DateTime? CreationDate { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ApplicationUser User { get; set; }
        public ICollection<MobilePartCategory> Categories { get; set; }
    }
    
    /// <summary>
    /// Represents a category for mobile parts
    /// </summary>
    public class MobilePartCategory
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public int? ParentID { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties
        public MobilePartCategory Parent { get; set; }
        public ICollection<MobilePartCategory> Children { get; set; }
    }
    
    /// <summary>
    /// Represents compatibility between a part and devices
    /// </summary>
    public class MobilePartCompatibility
    {
        public int CompatibilityID { get; set; }
        public int PartID { get; set; }
        public int BrandID { get; set; }
        public int? ModelID { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties
        public MobilePartAd Part { get; set; }
        public MobileBrand Brand { get; set; }
        public MobileModel Model { get; set; }
    }
}
