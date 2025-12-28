namespace GsmsharingV2.Models.NewSchema
{
    /// <summary>
    /// Reddit-style forum post with support for text, image, and link posts
    /// </summary>
    public class ForumPost
    {
        public long ForumPostID { get; set; }
        public long? UserId { get; set; }
        public long? CommunityID { get; set; }
        
        // Post Type: 'text', 'image', 'link', 'video'
        public string PostType { get; set; } = "text";
        
        // Content
        public string Title { get; set; }
        public string Content { get; set; } // For text posts
        public string Slug { get; set; }
        
        // Link Post Fields
        public string LinkUrl { get; set; }
        public string LinkTitle { get; set; }
        public string LinkDescription { get; set; }
        public string LinkThumbnail { get; set; }
        
        // Engagement Metrics
        public int ViewCount { get; set; } = 0;
        public int Score { get; set; } = 0;
        public int UpvoteCount { get; set; } = 0;
        public int DownvoteCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        
        // Status Flags
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public string PostStatus { get; set; } = "published"; // draft, published, archived
        
        // Timestamps
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Navigation Properties
        public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
        public ICollection<ForumPostVote> Votes { get; set; } = new List<ForumPostVote>();
    }
}


