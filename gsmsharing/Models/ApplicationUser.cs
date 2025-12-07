using Microsoft.AspNetCore.Identity;

namespace gsmsharing.Models
{
    public class ApplicationUser :IdentityUser
    {
        // Ban related properties
        public bool IsBanned { get; set; } = false;
        public DateTime? BannedAt { get; set; }
        public string? BanReason { get; set; }
        public string? BannedByUserId { get; set; }

        // Navigation properties
        public virtual ICollection<BanAppeal> BanAppeals { get; set; } = new List<BanAppeal>();
    }
}
