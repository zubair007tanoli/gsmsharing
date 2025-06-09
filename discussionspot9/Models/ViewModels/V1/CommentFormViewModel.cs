namespace discussionspot9.Models.ViewModels.V1
{
    public class CommentFormViewModel
    {
            public int PostId { get; set; }
            public string UserDisplayName { get; set; } = string.Empty;
            public string UserInitials { get; set; } = string.Empty;
        
    }
}
