namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Represents a cached link preview for a URL found in a comment.
    /// This enables performance optimization by storing fetched metadata.
    /// </summary>
    public class CommentLinkPreview
    {
        public int CommentLinkPreviewId { get; set; }
        public int CommentId { get; set; }
        public string Url { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastFetchedAt { get; set; }
        public bool FetchSucceeded { get; set; }

        // Navigation property
        public virtual Comment Comment { get; set; } = null!;
    }
}

