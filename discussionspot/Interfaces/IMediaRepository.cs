using discussionspot.Models.Domain;
using discussionspot.Repositories;

namespace discussionspot.Interfaces
{
    public interface IMediaRepository : IRepository<Media>
    {
        Task<IEnumerable<Media>> GetMediaForPostAsync(int postId);
        Task<IEnumerable<Media>> GetUserUploadsAsync(string userId, int page, int pageSize);
        Task<IEnumerable<Media>> GetMediaByTypeAsync(string mediaType, int page, int pageSize);
    }
}
