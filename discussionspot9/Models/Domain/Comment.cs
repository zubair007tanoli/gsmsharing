using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = null!;
        public string? UserId { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int Score { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public int TreeLevel { get; set; }

        // Navigation properties
        public virtual IdentityUser? User { get; set; }
        public virtual Post Post { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> ChildComments { get; set; } = new List<Comment>();
        public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
        public virtual ICollection<CommentAward> Awards { get; set; } = new List<CommentAward>();
    }
}
