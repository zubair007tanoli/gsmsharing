namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class ChatAdViewModel
    {
        public int ChatAdId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public string AdType { get; set; } = "banner";
        public string Placement { get; set; } = "chat-list";
        public bool IsSponsored { get; set; } = true;
    }
}

