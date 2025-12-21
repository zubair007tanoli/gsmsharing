namespace GsmsharingV2.Models
{
    public class SavedPost
    {
        public int SavedPostID { get; set; }
        public int PostID { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser User { get; set; }
    }
}

