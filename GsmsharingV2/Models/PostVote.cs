namespace GsmsharingV2.Models
{
    public class PostVote
    {
        public int VoteID { get; set; }
        public int PostID { get; set; }
        public string UserId { get; set; }
        public int VoteType { get; set; } // 1 = Upvote, -1 = Downvote
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser User { get; set; }
    }
}

