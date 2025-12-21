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
        
        // New modern fields
        public int UpvoteCount { get; set; } = 0;
        public int DownvoteCount { get; set; } = 0;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser User { get; set; }
        public Comment ParentComment { get; set; }
        public ICollection<CommentVote> Votes { get; set; }
        public ICollection<CommentReport> Reports { get; set; }
    }
}

