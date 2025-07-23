using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Models.ViewModels.HomePage;
using discussionspot9.Models.ViewModels.PollViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface IPostService
    {
        Task<PostListViewModel> GetAllPostsAsync(string sort = "hot", string time = "all", int page = 1);
        Task<PostListViewModel> GetCommunityPostsAsync(int communityId, string sort = "hot", int page = 1);
        Task<PostDetailViewModel?> GetPostDetailsAsync(string communitySlug, string postSlug);
        Task<PostDetailViewModel?> GetPostDetailsUpdateAsync(string communitySlug, string postSlug, string? currentUserId = null);
        Task IncrementViewCountAsync(int postId);
        Task<int?> GetUserVoteAsync(int postId, string userId);
        Task<CreatePostResult> CreatePostAsync(CreatePostViewModel model);
        Task<CreatePostResult> CreatePostUpdatedAsync(CreatePostViewModel model);
        Task<VoteResult> VotePostAsync(int postId, string userId, int voteType);
        Task<int> GetPostVoteCountAsync(int postId);
        Task IncrementShareCountAsync(int postId);
        Task<ServiceResult> DeletePostAsync(int postId, string userId);

        Task<bool> IsPostSavedByUserAsync(int postId, string userId);
        Task<SavePostResult> ToggleSavePostAsync(int postId, string userId);
        Task<List<TrendingTopicViewModel>> GetTrendingTopicsAsync();

        // IPostService.cs
        Task<PollViewModel?> GetPollDetailsAsync(int postId, string? userId);
        Task<VotePollResult> VotePollAsync(int postId, string userId, List<int> optionIds);
        Task<List<PostAwardViewModel>> GetPostAwardsAsync(int postId);
        Task<GiveAwardResult> GiveAwardAsync(int postId, int awardId, string userId, string? message);
        Task<PollViewModel?> GetPollDataAsync(int postId);
        Task<bool> HasUserVotedInPollAsync(int postId, string userId);
        Task<List<int>> GetUserPollVotesAsync(int postId, string userId);
        Task<Post> GetPostByIdAsync(int postId);
    }
}
