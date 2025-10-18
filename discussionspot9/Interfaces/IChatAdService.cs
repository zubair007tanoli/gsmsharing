using discussionspot9.Models.ViewModels.ChatViewModels;

namespace discussionspot9.Interfaces
{
    public interface IChatAdService
    {
        Task<ChatAdViewModel?> GetNextAdAsync(string userId, string placement, int messageCount);
        Task TrackImpressionAsync(int adId, string userId);
        Task TrackClickAsync(int adId, string userId);
        Task<List<ChatAdViewModel>> GetActiveAdsAsync(string placement);
    }
}

