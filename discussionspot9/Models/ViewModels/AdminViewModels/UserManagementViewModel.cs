namespace discussionspot9.Models.ViewModels.AdminViewModels
{
    public class UserManagementViewModel
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiresAt { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        
        public string FormattedJoinDate => JoinDate.ToString("MMM dd, yyyy");
        public string FormattedLastActive => GetTimeAgo(LastActive);
        public bool IsBanActive => IsBanned && (!BanExpiresAt.HasValue || BanExpiresAt.Value > DateTime.UtcNow);
        
        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
            return $"{(int)(timeSpan.TotalDays / 365)}y ago";
        }
    }
}

