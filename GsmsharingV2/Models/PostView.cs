namespace GsmsharingV2.Models
{
    public class PostView
    {
        public long ViewID { get; set; }
        public int PostID { get; set; }
        public string UserId { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string Referrer { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string DeviceType { get; set; } // desktop, mobile, tablet
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser User { get; set; }
    }
}

