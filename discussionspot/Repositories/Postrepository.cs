using discussionspot.Interfaces;
using discussionspot.ViewModels;

namespace discussionspot.Repositories
{
    public class Postrepository : IPostTasks
    {
        public Task<bool> CreatePostAsync(PostCreateViewModel postViewModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePostAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<PostCreateViewModel> GetPostByIdAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsByCategoryIdAsync(int categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsByCommunityIdAsync(int communityId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsByPaginationAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsBySearchTermAsync(string searchTerm)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsByTypeAsync(string type)
        {
            throw new NotImplementedException();
        }

        public Task<List<PostCreateViewModel>> GetPostsByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePostAsync(PostCreateViewModel postViewModel)
        {
            throw new NotImplementedException();
        }
    }
}
