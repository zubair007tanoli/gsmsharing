using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Models.ViewModels.HomePage;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface IPostService
    {
        Task<PostListViewModel> GetAllPostsAsync(string sort = "hot", string time = "all", int page = 1);
        Task<PostListViewModel> GetCommunityPostsAsync(int communityId, string sort = "hot", int page = 1);
        Task<PostDetailViewModel?> GetPostDetailsAsync(string communitySlug, string postSlug);
        Task IncrementViewCountAsync(int postId);
        Task<int?> GetUserVoteAsync(int postId, string userId);
        Task<CreatePostResult> CreatePostAsync(CreatePostViewModel model);
        Task<VoteResult> VotePostAsync(int postId, string userId, int voteType);
        Task<int> GetPostVoteCountAsync(int postId);
        Task IncrementShareCountAsync(int postId);
        Task<ServiceResult> DeletePostAsync(int postId, string userId);

        Task<bool> IsPostSavedByUserAsync(int postId, string userId);
        Task<SavePostResult> ToggleSavePostAsync(int postId, string userId);
        Task<List<Models.ViewModels.HomePage.TrendingTopicViewModel>> GetTrendingTopicsAsync();
    }
}
