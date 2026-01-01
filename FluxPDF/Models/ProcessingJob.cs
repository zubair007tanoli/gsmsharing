using System.ComponentModel.DataAnnotations;

namespace FluxPDF.Models
{
    public enum JobStatus { Pending = 0, Processing = 1, Completed = 2, Failed = 3, Cancelled = 4 }
    public enum JobType { Merge = 0, Split = 1, Convert = 2, Compress = 3, Enhance = 4, Extract = 5 }
    public class ProcessingJob
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public JobType JobType { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public int Progress { get; set; } = 0;
        [MaxLength(500)]
        public string? Message { get; set; }
        public string? InputFiles { get; set; }
        public string? OutputFile { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
