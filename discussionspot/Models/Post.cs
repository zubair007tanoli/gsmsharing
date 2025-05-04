using discussionspot.Models.Posts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient.Server;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public int CommunityId { get; set; }
        public string PostType { get; set; } = "text"; // text, link, image, video, poll
        public string Url { get; set; } // For link posts
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int UpvoteCount { get; set; } = 0;
        public int DownvoteCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public int Score { get; set; } = 0;
        public string Status { get; set; } = "published";
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsNSFW { get; set; } = false;
        public bool IsSpoiler { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public bool HasPoll { get; set; } = false;
        public int? PollOptionCount { get; set; }
        public int? PollVoteCount { get; set; }
        public DateTime? PollExpiresAt { get; set; }

        // Navigation properties
        public Community? Community { get; set; }
        public UserProfile? User { get; set; }
        public List<Media>? Media { get; set; }
        public List<Comment>? Comments { get; set; }
        public List<PostTag>? PostTags { get; set; }
        public SeoMetadata? SeoMetadata { get; set; }
        public PollConfiguration? PollConfiguration { get; set; }
        public List<PollOption>? PollOptions { get; set; }
    }
}
