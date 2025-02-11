using Microsoft.Extensions.Hosting;

namespace gsmsharing.Models
{
    public class Community
    {
        public int CommunityID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Rules { get; set; }
        public string CoverImage { get; set; }
        public string IconImage { get; set; }
        public string CreatorId { get; set; }
        public bool? IsPrivate { get; set; }
        public bool? IsVerified { get; set; }
        public int? MemberCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CategoryID { get; set; }

        // Navigation properties
        public Category Category { get; set; }
        public ApplicationUser Creator { get; set; }
        public ICollection<CommunityMember> Members { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<ChatRoom> ChatRooms { get; set; }    
    }
}
