using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface ISearchService
    {
        Task<(IEnumerable<PostViewModel> Posts, IEnumerable<CommunityViewModel> Communities, IEnumerable<UserViewModel> Users)> SearchAsync(string query, int page, int pageSize);
        Task<IEnumerable<PostViewModel>> SearchPostsAsync(string query, int page, int pageSize);
        Task<IEnumerable<CommunityViewModel>> SearchCommunitiesAsync(string query, int page, int pageSize);
        Task<IEnumerable<UserViewModel>> SearchUsersAsync(string query, int page, int pageSize);
        Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partial);
    }
}
