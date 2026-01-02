namespace GsmsharingV2.Models.NewSchema
{
    public class Post
    {
        public long PostID { get; set; }
        public long? UserId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Tags { get; set; }
        public string Content { get; set; }
        public string FeaturedImage { get; set; }
        public string Excerpt { get; set; }
        
        // SEO Fields
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string CanonicalUrl { get; set; }
        public string FocusKeyword { get; set; }
        public string SchemaMarkup { get; set; } // JSON-LD structured data
        
        // Status & Visibility
        public string PostStatus { get; set; } = "published"; // draft, published, archived
        public bool IsPromoted { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool AllowComments { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        
        // Engagement Metrics
        public int ViewCount { get; set; } = 0;
        public int Score { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public int UpvoteCount { get; set; } = 0;
        public int DownvoteCount { get; set; } = 0;
        
        // Relationships
        public long? CommunityID { get; set; }
        
        // Timestamps
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}









