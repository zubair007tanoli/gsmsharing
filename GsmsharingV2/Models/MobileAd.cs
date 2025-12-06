namespace GsmsharingV2.Models
{
    public class MobileAd
    {
        public int AdsId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Discription { get; set; }
        public int? Price { get; set; }
        public string Tags { get; set; }
        public int? Views { get; set; }
        public int? Likes { get; set; }
        public int? Dislikes { get; set; }
        public byte? Publish { get; set; }
        public DateTime? CreationDate { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public ICollection<AdImage> Images { get; set; }
    }
}

