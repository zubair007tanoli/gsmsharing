namespace discussionspot.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public string BannerUrl { get; set; }
        public string Website { get; set; }
        public string Location { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public List<CommunityMembershipViewModel> Communities { get; set; }
    }
}
