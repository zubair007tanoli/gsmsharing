namespace GsmsharingV2.Models.NewSchema
{
    public class ClassifiedAd
    {
        public long AdID { get; set; }
        public long UserID { get; set; }
        public int? CategoryID { get; set; }

        // Ad Details
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "PKR";

        // Product Specs
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Condition { get; set; }

        // Location
        public string City { get; set; }
        public string Area { get; set; }

        // Meta
        public string Status { get; set; } = "Active";
        public int ViewCount { get; set; } = 0;
        public int PhoneViewCount { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpirationDate { get; set; }

        // Navigation properties
        public AdCategory Category { get; set; }
        public ICollection<AdImage> Images { get; set; } = new List<AdImage>();
        public ICollection<SavedAd> SavedAds { get; set; } = new List<SavedAd>();
        public ICollection<ChatConversation> ChatConversations { get; set; } = new List<ChatConversation>();
    }
}

