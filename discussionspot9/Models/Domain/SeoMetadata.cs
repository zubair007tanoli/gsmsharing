namespace discussionspot9.Models.Domain
{
    public class SeoMetadata
    {
        public string EntityType { get; set; } = null!;
        public int EntityId { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }
        public string TwitterCard { get; set; } = "summary";
        public string? TwitterTitle { get; set; }
        public string? TwitterDescription { get; set; }
        public string? TwitterImageUrl { get; set; }
        public string? Keywords { get; set; }
        public string? StructuredData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
