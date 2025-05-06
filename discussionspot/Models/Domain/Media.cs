using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a media item attached to a post (image, video, document, audio)
    /// </summary>
    public class Media : BaseEntity
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        [StringLength(2048)]
        [Display(Name = "Media URL")]
        [DataType(DataType.Url)]
        public string Url { get; set; } = string.Empty;

        [StringLength(2048)]
        [Display(Name = "Thumbnail URL")]
        [DataType(DataType.Url)]
        public string? ThumbnailUrl { get; set; }

        [Display(Name = "Uploader")]
        public string? UserId { get; set; }

        [Display(Name = "Post")]
        public int? PostId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Media Type")]
        public string MediaType { get; set; } = string.Empty;  // image, video, document, audio

        [StringLength(100)]
        [Display(Name = "Content Type")]
        public string? ContentType { get; set; }  // MIME type

        [StringLength(255)]
        [Display(Name = "File Name")]
        public string? FileName { get; set; }

        [Display(Name = "File Size")]
        public long? FileSize { get; set; }

        [Display(Name = "Width")]
        public int? Width { get; set; }

        [Display(Name = "Height")]
        public int? Height { get; set; }

        [Display(Name = "Duration")]
        public int? Duration { get; set; }  // For video/audio in seconds

        [StringLength(500)]
        [Display(Name = "Caption")]
        public string? Caption { get; set; }

        [StringLength(500)]
        [Display(Name = "Alt Text")]
        public string? AltText { get; set; }  // For accessibility

        [Display(Name = "Upload Date")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        [Display(Name = "Storage Provider")]
        public string StorageProvider { get; set; } = "local";  // local, s3, cloudinary, etc.

        [StringLength(500)]
        [Display(Name = "Storage Path")]
        public string? StoragePath { get; set; }

        [Display(Name = "Processed")]
        public bool IsProcessed { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUsers? User { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        // Helper properties
        /// <summary>
        /// Gets a value indicating whether this media is an image
        /// </summary>
        [NotMapped]
        public bool IsImage => MediaType == "image";

        /// <summary>
        /// Gets a value indicating whether this media is a video
        /// </summary>
        [NotMapped]
        public bool IsVideo => MediaType == "video";

        /// <summary>
        /// Gets a value indicating whether this media is a document
        /// </summary>
        [NotMapped]
        public bool IsDocument => MediaType == "document";

        /// <summary>
        /// Gets a value indicating whether this media is an audio file
        /// </summary>
        [NotMapped]
        public bool IsAudio => MediaType == "audio";

        /// <summary>
        /// Gets a human-readable file size string
        /// </summary>
        [NotMapped]
        public string FileSizeFormatted
        {
            get
            {
                if (!FileSize.HasValue) return "Unknown";

                double bytes = FileSize.Value;

                if (bytes < 1024)
                    return $"{bytes} B";
                if (bytes < 1024 * 1024)
                    return $"{bytes / 1024:0.##} KB";
                if (bytes < 1024 * 1024 * 1024)
                    return $"{bytes / (1024 * 1024):0.##} MB";

                return $"{bytes / (1024 * 1024 * 1024):0.##} GB";
            }
        }

        /// <summary>
        /// Gets a human-readable duration string for audio/video
        /// </summary>
        [NotMapped]
        public string DurationFormatted
        {
            get
            {
                if (!Duration.HasValue) return "Unknown";

                int seconds = Duration.Value;

                int hours = seconds / 3600;
                int minutes = (seconds % 3600) / 60;
                int remainingSeconds = seconds % 60;

                if (hours > 0)
                    return $"{hours}:{minutes:00}:{remainingSeconds:00}";

                return $"{minutes}:{remainingSeconds:00}";
            }
        }
    }
}
