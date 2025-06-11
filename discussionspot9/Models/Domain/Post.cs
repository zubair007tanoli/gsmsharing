using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Content { get; set; }
        public string? UserId { get; set; }
        public int CommunityId { get; set; }
        public string PostType { get; set; } = "text";
        public string? Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int CommentCount { get; set; }
        public int Score { get; set; }
        public string Status { get; set; } = "published";
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public int ViewCount { get; set; }
        public bool HasPoll { get; set; }
        public int PollOptionCount { get; set; }
        public int PollVoteCount { get; set; }
        public DateTime? PollExpiresAt { get; set; }       
        public UserProfile UserProfile { get; set; }
        // Navigation properties
        public virtual IdentityUser? User { get; set; }
        public virtual Community Community { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostVote> Votes { get; set; } = new List<PostVote>();
        public virtual ICollection<Media> Media { get; set; } = new List<Media>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public virtual ICollection<PostAward> Awards { get; set; } = new List<PostAward>();
        public virtual ICollection<PollOption> PollOptions { get; set; } = new List<PollOption>();
        public virtual PollConfiguration? PollConfiguration { get; set; }
    }
}
