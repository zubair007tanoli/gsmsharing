using System;
using System.Collections.Generic;

namespace Fluxdox.Models
{
    public enum JobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Represents a background processing job for FluxDoc operations.
    /// </summary>
    public class JobModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public string? OperationType { get; set; } // e.g., "Merge", "OCR", "PdfToExcel"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> FileKeys { get; set; } = new List<string>(); // Input file keys
        public string? OutputFileKey { get; set; } // Key for the processed output file
        public string? ErrorMessage { get; set; }
        public int Progress { get; set; } // 0-100
    }
}