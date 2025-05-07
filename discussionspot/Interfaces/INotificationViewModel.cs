namespace discussionspot.Interfaces
{
    public class INotificationViewModel
    {
        int Id { get; set; }
        string UserId { get; set; }
        string Message { get; set; }
        string Type { get; set; }
        string? Link { get; set; }
        bool IsRead { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
