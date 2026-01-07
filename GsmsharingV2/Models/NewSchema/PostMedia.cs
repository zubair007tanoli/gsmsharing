namespace GsmsharingV2.Models.NewSchema
{
    /// <summary>
    /// Media attachments for posts and forum posts (images, videos)
    /// </summary>
    public class PostMedia
    {
        public long MediaID { get; set; }
        public long? PostID { get; set; } // Can link to Posts table
        public long? ForumPostID { get; set; } // Specific to forum posts
        public string MediaType { get; set; } // 'image', 'video', 'gif'
        public string MediaUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public long? FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Navigation Properties
        public ForumPost ForumPost { get; set; }
    }
}













