namespace GsmsharingV2.Models.NewSchema
{
    public class AdCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Slug { get; set; }
        public string IconClass { get; set; }

        // Navigation properties
        public ICollection<ClassifiedAd> ClassifiedAds { get; set; } = new List<ClassifiedAd>();
    }
}

