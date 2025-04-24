namespace discussionspot.ViewModels
{
    public class ModeratorDashboardViewModel
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public int TotalMembers { get; set; }
        public int NewMembersToday { get; set; }
        public int TotalPosts { get; set; }
        public int PostsToday { get; set; }
        public int TotalComments { get; set; }
        public int CommentsToday { get; set; }
        public int ReportedPosts { get; set; }
        public int ReportedComments { get; set; }
        public List<PostListItemViewModel> RecentPosts { get; set; }
        public List<CommunityMemberViewModel> Moderators { get; set; }
    }
}
