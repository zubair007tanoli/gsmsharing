namespace GsmsharingV2.Models
{
    public class Reaction
    {
        public int ReactionID { get; set; }
        public string UserId { get; set; }
        public int? PostID { get; set; }
        public int? CommentID { get; set; }
        public string ReactionType { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Post Post { get; set; }
        public Comment Comment { get; set; }
    }
}

