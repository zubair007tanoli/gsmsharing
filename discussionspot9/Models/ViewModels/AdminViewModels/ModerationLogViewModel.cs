namespace discussionspot9.Models.ViewModels.AdminViewModels
{
    public class ModerationLogViewModel
    {
        public long LogId { get; set; }
        public string? ModeratorName { get; set; }
        public string? TargetUserName { get; set; }
        public string? CommunityName { get; set; }
        public string ActionType { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime PerformedAt { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        public string FormattedDate => PerformedAt.ToString("MMM dd, yyyy HH:mm");
        public string TimeAgo => GetTimeAgo(PerformedAt);
        public string ActionTypeDisplay => FormatActionType(ActionType);
        public string ActionIcon => GetActionIcon(ActionType);
        public string ActionColor => GetActionColor(ActionType);
        
        private string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            return date.ToString("MMM dd, yyyy");
        }
        
        private string FormatActionType(string actionType)
        {
            return actionType.Replace("_", " ").ToUpper();
        }
        
        private string GetActionIcon(string actionType)
        {
            return actionType.ToLower() switch
            {
                "ban_site" or "ban_community" => "fa-ban",
                "unban_site" or "unban_community" => "fa-check-circle",
                "role_assign" => "fa-user-shield",
                "role_remove" => "fa-user-minus",
                "delete_post" => "fa-trash",
                "delete_comment" => "fa-comment-slash",
                _ => "fa-info-circle"
            };
        }
        
        private string GetActionColor(string actionType)
        {
            return actionType.ToLower() switch
            {
                "ban_site" or "ban_community" => "text-danger",
                "unban_site" or "unban_community" => "text-success",
                "role_assign" => "text-primary",
                "role_remove" => "text-warning",
                "delete_post" or "delete_comment" => "text-danger",
                _ => "text-info"
            };
        }
    }
}

