namespace discussionspot.Models.Posts
{
    public class SeoData
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = "post"; // Default entity type
        public int EntityId { get; set; } // Required foreign key for Posts or Communities
        public string Slug { get; set; } = string.Empty; // Prevent null values
        public string MetaTitle { get; set; } = string.Empty; // Default to empty (optional field)
        public string MetaDescription { get; set; } = string.Empty; // Default to empty (optional field)
        public string CanonicalUrl { get; set; } = string.Empty; // Default to empty (optional field)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default: Current UTC time
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Default: Current UTC time
    }
}
