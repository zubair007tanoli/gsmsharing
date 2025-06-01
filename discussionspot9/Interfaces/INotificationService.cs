namespace discussionspot9.Interfaces
{
    public interface INotificationService
    {
        Task CreateAwardNotificationAsync(int postId, int awardId, string fromUserId);
    }
}
