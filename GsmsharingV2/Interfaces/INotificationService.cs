namespace GsmsharingV2.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string content, string type, int? referenceId = null, string referenceType = null);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}


























