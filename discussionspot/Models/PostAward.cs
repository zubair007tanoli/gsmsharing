using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PostAward
    {
        [Key]
        public int PostAwardId { get; set; }

        public int PostId { get; set; }

        public int AwardId { get; set; }

        public string AwardedByUserId { get; set; }

        public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string Message { get; set; }

        public bool IsAnonymous { get; set; } = false;

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }

        [ForeignKey(nameof(AwardId))]
        public virtual Award Award { get; set; }

        [ForeignKey(nameof(AwardedByUserId))]
        public virtual IdentityUser AwardedByUser { get; set; }
    }
}
