namespace discussionspot9.Models.ViewModels.AdminViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int PendingOptimizations { get; set; }
        public int CompletedOptimizations { get; set; }
        public int TotalPosts { get; set; }
        public List<TopEarningPost> TopEarningPosts { get; set; } = new();
    }

    public class TopEarningPost
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public decimal Earnings { get; set; }
        public int Views { get; set; }
        public decimal RPM { get; set; }
    }
}

