namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    /// <summary>
    /// Request model for editing a comment
    /// </summary>
    public class EditCommentRequest
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for deleting a comment
    /// </summary>
    public class DeleteCommentRequest
    {
        public int CommentId { get; set; }
    }

    /// <summary>
    /// Request model for pinning/unpinning a comment
    /// </summary>
    public class PinCommentRequest
    {
        public int CommentId { get; set; }
    }
    
    /// <summary>
    /// Response model for pin toggle operation
    /// </summary>
    public class PinCommentResponse
    {
        public bool Success { get; set; }
        public bool IsPinned { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

