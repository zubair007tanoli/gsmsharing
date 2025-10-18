using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? CommunityId { get; set; } // Optional: Link to community
        public string CreatorId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public string? IconUrl { get; set; }

        // Navigation properties
        public virtual IdentityUser Creator { get; set; } = null!;
        public virtual Community? Community { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    }
}

