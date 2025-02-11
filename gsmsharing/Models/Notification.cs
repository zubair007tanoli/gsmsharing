namespace gsmsharing.Models
{
    public class Notification
    {
        int NotificationID { get; set; }
        string UserId { get; set; }
        string Title { get; set; }
        string Content { get; set; }
        string Type { get; set; }
        int? ReferenceID { get; set; }
        string ReferenceType { get; set; }
        bool? IsRead { get; set; }
        DateTime? CreatedAt { get; set; }
    }
}
