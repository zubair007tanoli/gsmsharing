namespace discussionspot.Interfaces
{
    public class IModerationLogViewModel
    {
        int Id { get; set; }
        string ModeratorId { get; set; }
        string? ModeratorName { get; set; }
        string Action { get; set; }
        string? Reason { get; set; }
        string? EntityType { get; set; }
        int EntityId { get; set; }
        string? TargetUserId { get; set; }
        string? TargetUserName { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
