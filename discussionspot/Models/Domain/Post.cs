using discussionspot.Models.Posts;
using Microsoft.Data.SqlClient.Server;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{

    /// <summary>
    /// Represents a post within a community
    /// </summary>
    public class Post : BaseEntity
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(320)]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Author")]
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "Community")]
        public int CommunityId { get; set; }

        [StringLength(20)]
        [Display(Name = "Post Type")]
        public string PostType { get; set; } = "text";  // text, link, image, video, poll

        [StringLength(2048)]
        [Display(Name = "External URL")]
        [DataType(DataType.Url)]
        public string? ExternalUrl { get; set; }  // For link posts - RENAMED from Url to ExternalUrl

        [Display(Name = "Upvotes")]
        public int UpvoteCount { get; set; } = 0;

        [Display(Name = "Downvotes")]
        public int DownvoteCount { get; set; } = 0;

        [Display(Name = "Comments")]
        public int CommentCount { get; set; } = 0;

        [Display(Name = "Score")]
        public int Score { get; set; } = 0;  // Calculated score

        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "published";  // published, removed, deleted, archived

        [Display(Name = "Pinned")]
        public bool IsPinned { get; set; } = false;

        [Display(Name = "Locked")]
        public bool IsLocked { get; set; } = false;

        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; } = false;

        [Display(Name = "Spoiler")]
        public bool IsSpoiler { get; set; } = false;

        [Display(Name = "Views")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Has Poll")]
        public bool HasPoll { get; set; } = false;

        [Display(Name = "Poll Options")]
        public int PollOptionCount { get; set; } = 0;

        [Display(Name = "Poll Votes")]
        public int PollVoteCount { get; set; } = 0;

        [Display(Name = "Poll Expires")]
        public DateTime? PollExpiresAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUsers? User { get; set; }

        [ForeignKey("CommunityId")]
        public virtual Community Community { get; set; } = null!;

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<PostVote>? Votes { get; set; }

        public virtual ICollection<Media>? Media { get; set; }

        public virtual ICollection<PostTag>? PostTags { get; set; }

        public virtual ICollection<PostAward>? Awards { get; set; }

        public virtual SeoMetadata? SeoMetadata { get; set; }

        public virtual PollConfiguration? PollConfiguration { get; set; }

        public virtual ICollection<PollOption>? PollOptions { get; set; }

        // Helper methods
        /// <summary>
        /// Generates a valid slug based on the post title
        /// </summary>
        public void GenerateSlug()
        {
            if (string.IsNullOrEmpty(Slug) && !string.IsNullOrEmpty(Title))
            {
                Slug = Title.ToLower()
                    .Replace(" ", "-")
                    .Replace("&", "and")
                    .Replace("/", "-")
                    .Replace("\\", "-")
                    .Replace(".", "")
                    .Replace(",", "")
                    .Replace(":", "")
                    .Replace(";", "")
                    .Replace("?", "")
                    .Replace("!", "")
                    .Replace("'", "")
                    .Replace("\"", "")
                    .Replace("(", "")
                    .Replace(")", "");

                // Limit slug length
                if (Slug.Length > 300)
                {
                    Slug = Slug.Substring(0, 300);
                }
            }
        }

        /// <summary>
        /// Gets the absolute URL for this post
        /// </summary>
        [NotMapped]
        public string PostUrl => $"/r/{Community?.Slug}/{Slug}";  // RENAMED from Url to PostUrl

        /// <summary>
        /// Gets the time ago string for this post (e.g. "5 hours ago")
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
        /// Gets a value indicating whether the post has media attachments
        /// </summary>
        [NotMapped]
        public bool HasMedia => Media != null && Media.Any();

        /// <summary>
        /// Gets the first media item for this post, if any
        /// </summary>
        [NotMapped]
        public Media? FirstMedia => Media?.FirstOrDefault();

        /// <summary>
        /// Returns a list of tags for this post
        /// </summary>
        [NotMapped]
        public IEnumerable<Tag> Tags => PostTags?.Select(pt => pt.Tag) ?? Enumerable.Empty<Tag>();
    }
}
