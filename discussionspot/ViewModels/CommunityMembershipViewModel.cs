namespace discussionspot.ViewModels
{
    public class CommunityMembershipViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string IconUrl { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
