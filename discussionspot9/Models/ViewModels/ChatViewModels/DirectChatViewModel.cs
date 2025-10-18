namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class DirectChatViewModel
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
        public string Status { get; set; } = "offline"; // online, away, busy, offline
        public int UnreadCount { get; set; }
        public ChatMessageViewModel? LastMessage { get; set; }
        public bool IsTyping { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}

