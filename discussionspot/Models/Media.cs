using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Media
    {
        public int MediaId { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string UserId { get; set; }
        public int? PostId { get; set; }
        public string MediaType { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
        public string Caption { get; set; }
        public string AltText { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public string StorageProvider { get; set; } = "local";
        public string StoragePath { get; set; }
        public bool IsProcessed { get; set; } = false;

    }
}
