using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Email queue for async email delivery with retry mechanism
    /// </summary>
    public class EmailQueue
    {
        [Key]
        public int EmailId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ToEmail { get; set; } = null!;

        [MaxLength(255)]
        public string? ToName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = null!;

        [Required]
        public string HtmlBody { get; set; } = null!;

        public string? PlainTextBody { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, sent, failed, cancelled

        public int RetryCount { get; set; } = 0;

        public int MaxRetries { get; set; } = 3;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        public DateTime? ScheduledFor { get; set; }

        public string? ErrorMessage { get; set; }

        [MaxLength(50)]
        public string? NotificationId { get; set; } // Link to notification if applicable

        [MaxLength(450)]
        public string? UserId { get; set; } // Recipient user ID

        [MaxLength(50)]
        public string EmailType { get; set; } = "notification"; // notification, digest, system, marketing

        public int Priority { get; set; } = 5; // 1 = highest, 10 = lowest
    }
}

