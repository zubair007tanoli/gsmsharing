namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class HeaderViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? UserId { get; set; }
        public int UnreadNotifications { get; set; }
        public int UnreadMessagesCount { get; set; }
        public List<NotificationViewModel> RecentNotifications { get; set; } = new();
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
    }
}
