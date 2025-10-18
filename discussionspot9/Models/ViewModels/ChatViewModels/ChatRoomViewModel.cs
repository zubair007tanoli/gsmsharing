namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class ChatRoomViewModel
    {
        public int ChatRoomId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int MemberCount { get; set; }
        public int UnreadCount { get; set; }
        public ChatMessageViewModel? LastMessage { get; set; }
        public bool IsPublic { get; set; }
        public bool IsMuted { get; set; }
        public List<string> OnlineMembers { get; set; } = new();
    }
}

