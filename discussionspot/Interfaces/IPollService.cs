using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IPollService
    {
        Task<PollConfigurationViewModel> GetPollConfigurationAsync(int postId);
        Task<IEnumerable<PollOptionViewModel>> GetPollOptionsAsync(int postId);
        Task<bool> VoteOnPollAsync(int postId, int optionId, string userId);
        Task<bool> CanUserVoteAsync(int postId, string userId);
        Task<IEnumerable<int>> GetUserVotesForPollAsync(int postId, string userId);
        Task<bool> CreatePollAsync(int postId, List<string> options, PollConfigurationViewModel config);
        Task<bool> AddPollOptionAsync(int postId, string optionText, string userId);
        Task<bool> DeletePollOptionAsync(int optionId, string userId);
    }
}
