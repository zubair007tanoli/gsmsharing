using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient.Server;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Community
    {
        [Key]
        public int CommunityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(120)]
        public string Slug { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; }

        public int? CategoryId { get; set; }

        public string CreatorId { get; set; }

        [StringLength(20)]
        public string CommunityType { get; set; } = "public";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(2048)]
        public string IconUrl { get; set; }

        [StringLength(2048)]
        public string BannerUrl { get; set; }

        [StringLength(20)]
        public string ThemeColor { get; set; }

        public int MemberCount { get; set; } = 0;

        public int PostCount { get; set; } = 0;

        public string Rules { get; set; }

        public bool IsNSFW { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }

        [ForeignKey(nameof(CreatorId))]
        public virtual IdentityUser Creator { get; set; }

        // Navigation properties
        public virtual ICollection<CommunityMember> Members { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual SeoMetadata SeoMetadata { get; set; }

    }
}
