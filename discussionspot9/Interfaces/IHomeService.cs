using discussionspot9.Models.ViewModels.HomePage;

namespace discussionspot9.Interfaces
{
    public interface IHomeService
    {
        Task<HomePageViewModel> GetHomePageDataAsync();
        Task<List<RandomPostViewModel>> GetRandomPostsAsync(int count = 4);
        Task<List<CategoryViewModel>> GetCategoriesAsync();
        Task<List<RecentPostViewModel>> GetRecentPostsAsync(int count = 3);
        Task<SidebarViewModel> GetSidebarDataAsync();
    }
}
