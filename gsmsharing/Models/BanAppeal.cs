using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gsmsharing.Models
{
    public class BanAppeal
    {
        [Key]
        public int BanAppealId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(2000)]
        public string AppealMessage { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedByUserId { get; set; }

        public AppealStatus Status { get; set; } = AppealStatus.Pending;

        [StringLength(500)]
        public string? AdminResponse { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("ReviewedByUserId")]
        public virtual ApplicationUser? ReviewedBy { get; set; }
    }

    public enum AppealStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}


