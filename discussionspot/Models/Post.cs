using discussionspot.Models.Posts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient.Server;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        [StringLength(320)]
        public string Slug { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public int CommunityId { get; set; }

        [StringLength(20)]
        public string PostType { get; set; } = "text";

        [StringLength(2048)]
        public string Url { get; set; } 
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UpvoteCount { get; set; } = 0;

        public int DownvoteCount { get; set; } = 0;

        public int CommentCount { get; set; } = 0;

        public int Score { get; set; } = 0;

        [StringLength(20)]
        public string Status { get; set; } = "published";

        public bool IsPinned { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        public bool IsNSFW { get; set; } = false;

        public bool IsSpoiler { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public bool HasPoll { get; set; } = false;

        public int PollOptionCount { get; set; } = 0;

        public int PollVoteCount { get; set; } = 0;

        public DateTime? PollExpiresAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        [ForeignKey(nameof(CommunityId))]
        public virtual Community Community { get; set; }

        // Navigation properties
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<PostVote> Votes { get; set; }
        public virtual ICollection<Media> Media { get; set; }
        public virtual ICollection<PostTag> Tags { get; set; }
        public virtual ICollection<PostAward> Awards { get; set; }
        public virtual PollConfiguration PollConfiguration { get; set; }
        public virtual ICollection<PollOption> PollOptions { get; set; }
        public virtual SeoMetadata SeoMetadata { get; set; }
    }
}
