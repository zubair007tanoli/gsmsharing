using Microsoft.AspNetCore.Identity;

namespace FluxPDF.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPremium { get; set; } = false;
        public DateTime? PremiumExpiresAt { get; set; }
        
        public virtual ICollection<FileMetadata> Files { get; set; } = new List<FileMetadata>();
        public virtual ICollection<ProcessingJob> ProcessingJobs { get; set; } = new List<ProcessingJob>();
        public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    }
}
