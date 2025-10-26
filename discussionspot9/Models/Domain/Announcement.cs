using System;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Type of announcement: info, success, warning, danger
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = "info";

        /// <summary>
        /// Icon class (FontAwesome)
        /// </summary>
        [MaxLength(50)]
        public string Icon { get; set; } = "fa-info-circle";

        /// <summary>
        /// Optional link URL
        /// </summary>
        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Link text if LinkUrl is provided
        /// </summary>
        [MaxLength(100)]
        public string? LinkText { get; set; }

        /// <summary>
        /// Whether the announcement is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether users can dismiss this announcement
        /// </summary>
        public bool IsDismissible { get; set; } = true;

        /// <summary>
        /// Priority/Order for display (higher = shown first)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Start date for showing the announcement
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for showing the announcement
        /// </summary>
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }
    }
}

