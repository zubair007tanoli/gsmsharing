namespace discussionspot.ViewModels
{
    public class PollVoteViewModel
    {
        public int PostId { get; set; }
        public List<int> SelectedOptionIds { get; set; }
    }
}
