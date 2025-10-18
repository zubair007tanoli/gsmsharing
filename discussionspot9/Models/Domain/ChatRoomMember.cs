using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class ChatRoomMember
    {
        public int ChatRoomMemberId { get; set; }
        public int ChatRoomId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public string Role { get; set; } = "member"; // member, moderator, admin
        public bool IsMuted { get; set; }
        public DateTime? LastReadAt { get; set; }

        // Navigation properties
        public virtual ChatRoom ChatRoom { get; set; } = null!;
        public virtual IdentityUser User { get; set; } = null!;
    }
}

