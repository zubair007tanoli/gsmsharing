namespace discussionspot9.Models.ViewModels
{
    public class UserStatsViewModel
    {
        public string DisplayName { get; set; } = null!;
        public int KarmaPoints { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsVerified { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
