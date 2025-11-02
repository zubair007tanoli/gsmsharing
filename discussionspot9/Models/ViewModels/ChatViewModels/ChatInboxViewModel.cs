namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class ChatInboxViewModel
    {
        public List<DirectChatViewModel> DirectChats { get; set; } = new();
        public List<ChatRoomViewModel> ChatRooms { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}

