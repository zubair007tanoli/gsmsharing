using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class Community
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public int? CategoryId { get; set; }
        public string? CreatorId { get; set; }
        public string CommunityType { get; set; } = "public";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? IconUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? ThemeColor { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public string? Rules { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public virtual Category? Category { get; set; }
        public virtual IdentityUser? Creator { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();
    }
}
