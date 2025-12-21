namespace GsmsharingV2.Models
{
    public class PostHistory
    {
        public int HistoryID { get; set; }
        public int PostID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string EditedBy { get; set; }
        public string EditReason { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser Editor { get; set; }
    }
}

