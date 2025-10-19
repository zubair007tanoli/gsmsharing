namespace discussionspot9.Models.ViewModels.AdminViewModels
{
    public class AdminStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; } // Active in last 30 days
        public int BannedUsers { get; set; }
        public int TotalCommunities { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ModerationActionsToday { get; set; }
        public int ModerationActionsThisWeek { get; set; }
        public List<TopModeratorViewModel> TopModerators { get; set; } = new List<TopModeratorViewModel>();
    }
    
    public class TopModeratorViewModel
    {
        public string ModeratorId { get; set; } = null!;
        public string ModeratorName { get; set; } = null!;
        public int ActionCount { get; set; }
    }
}

