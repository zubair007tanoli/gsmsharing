using discussionspot9.Models.Domain;

namespace discussionspot9.Models.ViewModels.AdminViewModels
{
    public class RevenueViewModel
    {
        public List<AdSenseRevenue> RevenueData { get; set; } = new();
        public List<TopEarningPost> TopEarningPosts { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal AvgDailyRevenue { get; set; }
    }
}

