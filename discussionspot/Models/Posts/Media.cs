namespace discussionspot.Models.Posts
{
    public class Media
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty; // Prevent null values
        public string UploaderId { get; set; } // Allow null (foreign key constraint)
        public string MediaType { get; set; } = "image"; // Default media type
        public string Caption { get; set; } = string.Empty; // Optional, default to empty
        public int? Width { get; set; } // Nullable for unsupported dimensions
        public int? Height { get; set; } // Nullable for unsupported dimensions
        public long? FileSize { get; set; } // Nullable for optional file size
        public int PostId { get; set; } // Required foreign key for Posts
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default: Current UTC time
    }
}
