namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PostAwardViewModel
    {
        public int AwardId { get; set; }
        public string AwardName { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public string? AwardedByDisplayName { get; set; }
        public DateTime AwardedAt { get; set; }
        public string? Message { get; set; }
        public bool IsAnonymous { get; set; }

        public string AwardIconUrl { get; set; } = string.Empty;
        public string GivenBy { get; set; } = string.Empty;
        public DateTime DateGiven { get; set; }
  
    }
}
