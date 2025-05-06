using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a comment on a post
    /// </summary>
    public class Comment : BaseEntity
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        [Display(Name = "Comment Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Author")]
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "Post")]
        public int PostId { get; set; }

        [Display(Name = "Parent Comment")]
        public int? ParentCommentId { get; set; }

        [Display(Name = "Upvotes")]
        public int UpvoteCount { get; set; } = 0;

        [Display(Name = "Downvotes")]
        public int DownvoteCount { get; set; } = 0;

        [Display(Name = "Score")]
        public int Score { get; set; } = 0;

        [Display(Name = "Edited")]
        public bool IsEdited { get; set; } = false;

        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Tree Level")]
        public int TreeLevel { get; set; } = 0;  // Comment hierarchy level

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUsers? User { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment>? ChildComments { get; set; }

        public virtual ICollection<CommentVote>? Votes { get; set; }

        public virtual ICollection<CommentAward>? Awards { get; set; }

        // Helper properties
        /// <summary>
        /// Gets the time ago string for this comment (e.g. "5 minutes ago")
        /// </summary>
        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;

                if (timeSpan.TotalSeconds < 60)
                    return $"{(int)timeSpan.TotalSeconds} seconds ago";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} days ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} months ago";

                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this comment has been edited after creation
        /// </summary>
        [NotMapped]
        public bool WasEdited => IsEdited || (UpdatedAt - CreatedAt).TotalSeconds > 60;

        /// <summary>
        /// Gets the display text for this comment, with "[deleted]" if applicable
        /// </summary>
        [NotMapped]
        public string DisplayContent => IsDeleted ? "[deleted]" : Content;

        /// <summary>
        /// Gets a value indicating whether this comment has replies
        /// </summary>
        [NotMapped]
        public bool HasReplies => ChildComments != null && ChildComments.Any();

        /// <summary>
        /// Gets the number of replies to this comment
        /// </summary>
        [NotMapped]
        public int ReplyCount => ChildComments?.Count ?? 0;
    }
}
