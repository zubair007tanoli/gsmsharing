using discussionspot.Models.ViewModels;

namespace discussionspot.Interfaces
{
    public interface IMediaService
    {
        Task<int> UploadMediaAsync(IFormFile file, string userId, int? postId = null);
        Task<bool> DeleteMediaAsync(int mediaId, string userId);
        Task<MediaViewModel> GetMediaByIdAsync(int mediaId);
        Task<IEnumerable<MediaViewModel>> GetMediaForPostAsync(int postId);
        Task<string?> GenerateThumbnailAsync(int mediaId);
        Task<IEnumerable<MediaViewModel>> GetUserMediaAsync(string userId, int page, int pageSize);
        Task<bool> ProcessVideoAsync(int mediaId);
        Task<bool> UpdateMediaMetadataAsync(int mediaId, MediaViewModel model);
    }
}
