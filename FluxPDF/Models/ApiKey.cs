using System.ComponentModel.DataAnnotations;

namespace FluxPDF.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Key { get; set; } = Guid.NewGuid().ToString("N");
        [MaxLength(200)]
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public bool IsActive { get; set; } = true;
        public int RateLimit { get; set; } = 1000;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
