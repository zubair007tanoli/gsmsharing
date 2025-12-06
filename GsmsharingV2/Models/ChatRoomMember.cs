namespace GsmsharingV2.Models
{
    public class ChatRoomMember
    {
        public int RoomID { get; set; }
        public string UserId { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? LastReadAt { get; set; }

        // Navigation properties
        public ChatRoom Room { get; set; }
        public ApplicationUser User { get; set; }
    }
}

