namespace GsmsharingV2.Models
{
    /// <summary>
    /// Represents a mobile device brand (e.g., Samsung, Apple, Xiaomi)
    /// </summary>
    public class MobileBrand
    {
        public int BrandID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties
        public ICollection<MobileModel> Models { get; set; }
    }
}
