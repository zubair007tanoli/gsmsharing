namespace discussionspot.ViewModels
{
    public class CommunityViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string CommunityType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IconUrl { get; set; }
        public string BannerUrl { get; set; }
        public string ThemeColor { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public string Rules { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUserMember { get; set; }
        public string UserRole { get; set; }
        public List<PostListItemViewModel> RecentPosts { get; set; }
    }
}
