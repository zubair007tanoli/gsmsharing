namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class DirectChatPageViewModel
    {
        public string UserId { get; set; } = null!;
        public string? UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}

