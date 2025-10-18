namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Smart, non-intrusive ads for chat system
    /// </summary>
    public class ChatAd
    {
        public int ChatAdId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public string AdType { get; set; } = "banner"; // banner, sponsored-message, inline
        public string Placement { get; set; } = "chat-list"; // chat-list, chat-window, sidebar
        public int DisplayFrequency { get; set; } = 10; // Show every X messages
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int ImpressionCount { get; set; }
        public int ClickCount { get; set; }
        public decimal Budget { get; set; }
        public decimal CostPerImpression { get; set; }

        // Targeting
        public string? TargetAudience { get; set; } // JSON for demographics
        public int MinMessages { get; set; } // Minimum messages before showing ad
    }
}

