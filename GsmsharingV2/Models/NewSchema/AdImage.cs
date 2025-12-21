namespace GsmsharingV2.Models.NewSchema
{
    public class AdImage
    {
        public long ImageID { get; set; }
        public long AdID { get; set; }
        public string ImagePath { get; set; }
        public bool IsPrimary { get; set; } = false;

        // Navigation properties
        public ClassifiedAd Ad { get; set; }
    }
}

