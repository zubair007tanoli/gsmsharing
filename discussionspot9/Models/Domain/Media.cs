using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class Media
    {
        public int MediaId { get; set; }
        public string Url { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }
        public string? UserId { get; set; }
        public int? PostId { get; set; }
        public string MediaType { get; set; } = null!;
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
        public string? Caption { get; set; }
        public string? AltText { get; set; }
        public DateTime UploadedAt { get; set; }
        public string StorageProvider { get; set; } = "local";
        public string? StoragePath { get; set; }
        public bool IsProcessed { get; set; }

        // Navigation properties
        public virtual IdentityUser? User { get; set; }
        public virtual Post? Post { get; set; }
        //public int? DisplayOrder { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        //public DateTime UpdatedAt { get; set; }
    }
}
