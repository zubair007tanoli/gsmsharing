namespace discussionspot9.Models.Domain
{
    public class UserProfile
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
    }
}
