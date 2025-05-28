namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class MemberViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
