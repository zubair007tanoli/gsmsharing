using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class PostReport
    {
        public int ReportId { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public string? Details { get; set; }
        public string Status { get; set; } = "pending"; // pending, reviewed, resolved, dismissed
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByUserId { get; set; }
        public string? AdminNotes { get; set; }

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual IdentityUser User { get; set; } = null!;
        public virtual IdentityUser? ReviewedBy { get; set; }
    }
}

