using System;
using System.Collections.Generic;

namespace Fluxdox.Models
{
    public enum JobStatus { Pending, Running, Completed, Failed }

    public class JobModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public JobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> FileKeys { get; set; } = new List<string>();
        public string ResultKey { get; set; }
    }
}