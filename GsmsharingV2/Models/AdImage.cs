namespace GsmsharingV2.Models
{
    public class AdImage
    {
        public int SalePicId { get; set; }
        public int? AdsId { get; set; }
        public byte[] Pics { get; set; }
        public DateTime? ImageDate { get; set; }

        // Navigation properties
        public MobileAd Ad { get; set; }
    }
}

