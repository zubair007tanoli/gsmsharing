namespace discussionspot.Interfaces
{
    public interface IPostTasks
    {
        public Task<Boolean> CreatePostAsync(PostViewModel postViewModel);
        public Task<Boolean> UpdatePostAsync(PostViewModel postViewModel);
        public Task<Boolean> DeletePostAsync(int postId);
        public Task<PostViewModel> GetPostByIdAsync(int postId);
        public Task<List<PostViewModel>> GetPostsByUserIdAsync(string userId);
        public Task<List<PostViewModel>> GetPostsByCommunityIdAsync(int communityId);
        public Task<List<PostViewModel>> GetPostsByCategoryIdAsync(int categoryId);
        public Task<List<PostViewModel>> GetPostsBySearchTermAsync(string searchTerm);
        public Task<List<PostViewModel>> GetPostsByStatusAsync(string status);
        public Task<List<PostViewModel>> GetPostsByTypeAsync(string type);
        public Task<List<PostViewModel>> GetPostsByPaginationAsync(int pageNumber, int pageSize);


    }
}
