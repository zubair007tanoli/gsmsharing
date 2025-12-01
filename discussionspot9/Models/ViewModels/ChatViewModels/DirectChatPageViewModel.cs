namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class DirectChatPageViewModel
    {
        public string OtherUserId { get; set; } = null!;
        public string OtherUserName { get; set; } = "User";
        public string? OtherUserAvatar { get; set; }
        public string CurrentUserId { get; set; } = null!;
        public List<ChatMessageViewModel> Messages { get; set; } = new();
    }
}
