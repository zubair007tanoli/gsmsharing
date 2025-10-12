using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Queue for posts needing SEO optimization
    /// </summary>
    public class PostSeoQueue
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        public int Priority { get; set; } // 1=Critical, 2=High, 3=Medium, 4=Low
        
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;
        
        public decimal? EstimatedRevenueImpact { get; set; }
        
        public DateTime AddedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed, Skipped
        
        public string? ErrorMessage { get; set; }
        
        // Suggested changes (JSON)
        public string? SuggestedChanges { get; set; }
        
        public bool RequiresApproval { get; set; }
        
        // Navigation
        public virtual Post Post { get; set; } = null!;
    }
}

