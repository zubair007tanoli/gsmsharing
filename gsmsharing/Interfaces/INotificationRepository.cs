using gsmsharing.Models;

namespace gsmsharing.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetByIdAsync(int id);
        Task<IEnumerable<Notification>> GetAllAsync();
        Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Notification>> GetByTypeAsync(string type);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task DeleteAsync(int id);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<Notification>> GetPaginatedAsync(int page, int pageSize);
        Task<int> GetUnreadCountForUserAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadForUserAsync(string userId);
        Task<IEnumerable<Notification>> GetByReferenceIdAndTypeAsync(int referenceId, string referenceType);
    }
}
