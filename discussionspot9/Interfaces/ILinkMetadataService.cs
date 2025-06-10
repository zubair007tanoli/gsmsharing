using discussionspot9.Models.ViewModels.CreativeViewModels;

namespace discussionspot9.Interfaces
{
    public interface ILinkMetadataService
    {
        Task<LinkPreviewViewModel> GetMetadataAsync(string url);
    }
}
