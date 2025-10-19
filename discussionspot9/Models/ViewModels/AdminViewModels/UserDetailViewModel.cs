namespace discussionspot9.Models.ViewModels.AdminViewModels
{
    public class UserDetailViewModel : UserManagementViewModel
    {
        public string? Bio { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public string? BannerUrl { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunityCount { get; set; }
        public List<string> CommunityRoles { get; set; } = new List<string>();
        public List<ModerationLogViewModel> RecentLogs { get; set; } = new List<ModerationLogViewModel>();
    }
}

