using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class BanAppeal
    {
        [Key]
        public int BanAppealId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string AppealMessage { get; set; } = null!;

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        [StringLength(450)]
        public string? ReviewedByUserId { get; set; }

        public AppealStatus Status { get; set; } = AppealStatus.Pending;

        [StringLength(500)]
        public string? AdminResponse { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        [ForeignKey("ReviewedByUserId")]
        public virtual IdentityUser? ReviewedBy { get; set; }
    }

    public enum AppealStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}


