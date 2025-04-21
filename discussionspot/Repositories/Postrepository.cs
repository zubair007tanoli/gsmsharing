using discussionspot.Interfaces;

namespace discussionspot.Repositories
{
    public class Postrepository : IPostTasks
    {
        public Task<bool> CreatePostAsync(PostViewModel postViewModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePostAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<PostViewModel> GetPostByIdAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsByCategoryIdAsync(int categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsByCommunityIdAsync(int communityId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsByPaginationAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsBySearchTermAsync(string searchTerm)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsByTypeAsync(string type)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostViewModel>> GetPostsByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePostAsync(PostViewModel postViewModel)
        {
            throw new NotImplementedException();
        }
    }
}
