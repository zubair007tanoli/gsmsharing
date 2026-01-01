using System.ComponentModel.DataAnnotations;

namespace FluxPDF.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        [Required, MaxLength(500)]
        public string FileName { get; set; } = string.Empty;
        [Required, MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? MimeType { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public bool IsPermanent { get; set; } = false;
        public virtual ApplicationUser? User { get; set; }
    }
}
