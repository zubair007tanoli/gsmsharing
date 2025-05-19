namespace discussionspot.Models.ViewModels
{
    public class PollOptionViewModel
    {
        public int PollOptionId { get; set; }
        public int PostId { get; set; }
        public string OptionText { get; set; }
        public int DisplayOrder { get; set; }
        public int VoteCount { get; set; }
        public bool IsVotedByUser { get; set; }
        public double Percentage { get; set; }
    }
}
