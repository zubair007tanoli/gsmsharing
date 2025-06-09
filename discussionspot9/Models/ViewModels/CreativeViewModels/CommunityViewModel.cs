namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int OnlineCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMember { get; set; }
    }
}
