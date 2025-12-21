namespace GsmsharingV2.Models.NewSchema
{
    public class AffiliatePartner
    {
        public int PartnerID { get; set; }
        public string Name { get; set; }
        public string AffiliateTag { get; set; }
        public string BaseUrl { get; set; }

        // Navigation properties
        public ICollection<AffiliateProductNew> Products { get; set; } = new List<AffiliateProductNew>();
    }
}

