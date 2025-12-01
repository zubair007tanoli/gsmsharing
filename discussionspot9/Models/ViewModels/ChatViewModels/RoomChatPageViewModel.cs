namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class RoomChatPageViewModel
    {
        public ChatRoomViewModel Room { get; set; } = null!;
        public string CurrentUserId { get; set; } = null!;
        public List<ChatMessageViewModel> Messages { get; set; } = new();
    }
}

