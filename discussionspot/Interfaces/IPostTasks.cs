using discussionspot.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IPostTasks
    {
        public Task<Boolean> CreatePostAsync(PostCreateViewModel postViewModel);
        public Task<Boolean> UpdatePostAsync(PostCreateViewModel postViewModel);
        public Task<Boolean> DeletePostAsync(int postId);
        public Task<PostCreateViewModel> GetPostByIdAsync(int postId);
        public Task<List<PostCreateViewModel>> GetPostsByUserIdAsync(string userId);
        public Task<List<PostCreateViewModel>> GetPostsByCommunityIdAsync(int communityId);
        public Task<List<PostCreateViewModel>> GetPostsByCategoryIdAsync(int categoryId);
        public Task<List<PostCreateViewModel>> GetPostsBySearchTermAsync(string searchTerm);
        public Task<List<PostCreateViewModel>> GetPostsByStatusAsync(string status);
        public Task<List<PostCreateViewModel>> GetPostsByTypeAsync(string type);
        public Task<List<PostCreateViewModel>> GetPostsByPaginationAsync(int pageNumber, int pageSize);


    }
}
