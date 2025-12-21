namespace GsmsharingV2.Models.NewSchema
{
    public class AdminLog
    {
        public long LogID { get; set; }
        public long? AdminUserID { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime LogDate { get; set; } = DateTime.UtcNow;

    }
}

