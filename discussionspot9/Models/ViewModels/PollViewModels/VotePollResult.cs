namespace discussionspot9.Models.ViewModels.PollViewModels
{
    public class VotePollResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Dictionary<int, int>? UpdatedVoteCounts { get; set; }
    }
}
