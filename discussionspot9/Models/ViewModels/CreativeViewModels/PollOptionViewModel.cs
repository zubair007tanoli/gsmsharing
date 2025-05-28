namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PollOptionViewModel
    {
        public int PollOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public double VotePercentage { get; set; }
        public bool IsUserSelection { get; set; }
    }
}
