namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityListItemViewModel
    {
        public int CommunityId { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public required string Description { get; set; }
        public required string IconUrl { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public int OnlineMembers { get; set; }
        public List<string> Categories { get; set; } = new();
        public required string CategoryName { get; set; }
        public bool IsCurrentUserMember { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
