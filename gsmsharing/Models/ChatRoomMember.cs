namespace gsmsharing.Models
{
    public class ChatRoomMember
    {
        int RoomID { get; set; }
        string UserId { get; set; }
        DateTime? JoinedAt { get; set; }
        DateTime? LastReadAt { get; set; }
    }
}
