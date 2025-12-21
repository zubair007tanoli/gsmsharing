namespace GsmsharingV2.Models.NewSchema
{
    public class SavedAd
    {
        public long SavedID { get; set; }
        public long UserID { get; set; }
        public long AdID { get; set; }
        public DateTime SavedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ClassifiedAd Ad { get; set; }
    }
}

