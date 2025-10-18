namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class ChatMessageViewModel
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string SenderAvatar { get; set; } = null!;
        public string? ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; } // Is current user the sender
        public string TimeAgo { get; set; } = string.Empty;
        public string FormattedTime { get; set; } = string.Empty;
    }
}

