
namespace discussionspot9.Interfaces
{
    public interface INotificationService
    {
        Task CreateAwardNotificationAsync(int postId, int awardId, string fromUserId);
        Task CreateNotificationAsync(object userId, string v1, int postId, string v2);
        Task<CancellationToken> GetUnreadCountAsync(string? userId);
        Task MarkAsReadAsync(int notificationId, string? userId);

    }
}
