namespace gsmsharing.Models
{
    public class ChatRoom
    {
        public int RoomID { get; set; }
        public string RoomType { get; set; }
        public int? CommunityID { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }

        // Navigation properties
        public Community Community { get; set; }
        public ApplicationUser Creator { get; set; }
    }
}
