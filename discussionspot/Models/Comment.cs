using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        public string UserId { get; set; }

        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UpvoteCount { get; set; } = 0;

        public int DownvoteCount { get; set; } = 0;

        public int Score { get; set; } = 0;

        public bool IsEdited { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public int TreeLevel { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public virtual Comment ParentComment { get; set; }

        // Navigation properties
        public virtual ICollection<Comment> ChildComments { get; set; }
        public virtual ICollection<CommentVote> Votes { get; set; }
        public virtual ICollection<CommentAward> Awards { get; set; }
    }
}
