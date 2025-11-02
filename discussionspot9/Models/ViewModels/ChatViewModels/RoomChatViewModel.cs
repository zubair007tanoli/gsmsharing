namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class RoomChatViewModel
    {
        public ChatRoomViewModel Room { get; set; } = null!;
        public List<ChatMessageViewModel> Messages { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}

