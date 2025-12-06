namespace GsmsharingV2.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public int? ReferenceID { get; set; }
        public string ReferenceType { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
    }
}

