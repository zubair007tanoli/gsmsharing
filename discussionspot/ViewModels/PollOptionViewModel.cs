namespace discussionspot.ViewModels
{
    public class PollOptionViewModel
    {
        public int PollOptionId { get; set; }
        public string OptionText { get; set; }
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
        public bool UserVoted { get; set; }
    }
}
