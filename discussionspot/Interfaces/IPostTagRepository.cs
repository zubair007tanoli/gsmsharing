using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IPostTagRepository : IRepository<PostTag>
    {
        Task<IEnumerable<Tag>> GetTagsForPostAsync(int postId);
        Task<IEnumerable<Post>> GetPostsByTagAsync(int tagId, int page, int pageSize);
        Task RemoveAllTagsFromPostAsync(int postId);
    }
}
