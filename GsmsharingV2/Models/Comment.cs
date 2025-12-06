namespace GsmsharingV2.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int? PostID { get; set; }
        public string UserId { get; set; }
        public int? ParentCommentID { get; set; }
        public string Content { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser User { get; set; }
        public Comment ParentComment { get; set; }
    }
}

