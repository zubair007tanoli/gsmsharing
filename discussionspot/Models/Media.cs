using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Media
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        [StringLength(2048)]
        public string Url { get; set; }

        [StringLength(2048)]
        public string ThumbnailUrl { get; set; }

        public string UserId { get; set; }

        public int? PostId { get; set; }

        [Required]
        [StringLength(20)]
        public string MediaType { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        public long? FileSize { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Duration { get; set; }

        [StringLength(500)]
        public string Caption { get; set; }

        [StringLength(500)]
        public string AltText { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string StorageProvider { get; set; } = "local";

        [StringLength(500)]
        public string StoragePath { get; set; }

        public bool IsProcessed { get; set; } = false;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }

    }
}
