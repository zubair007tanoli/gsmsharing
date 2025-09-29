namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public int OnlineMembers { get; set; }
        public List<string> Categories { get; set; } = new();    
        public bool IsUserMember { get; set; }
    }
}
