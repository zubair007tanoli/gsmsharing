namespace GsmsharingV2.Models
{
    public class CommunityMember
    {
        public int CommunityMemberID { get; set; }
        public int CommunityID { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public DateTime? JoinedAt { get; set; }

        // Navigation properties
        public Community Community { get; set; }
        public ApplicationUser User { get; set; }
    }
}

