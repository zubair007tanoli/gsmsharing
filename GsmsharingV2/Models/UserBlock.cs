namespace GsmsharingV2.Models
{
    public class UserBlock
    {
        public int BlockID { get; set; }
        public string BlockerUserId { get; set; } // User who is blocking
        public string BlockedUserId { get; set; } // User who is being blocked
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public ApplicationUser Blocker { get; set; }
        public ApplicationUser Blocked { get; set; }
    }
}

