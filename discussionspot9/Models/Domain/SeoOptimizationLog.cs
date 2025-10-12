using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Logs all SEO optimization changes
    /// </summary>
    public class SeoOptimizationLog
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        public DateTime OptimizedAt { get; set; }
        
        [StringLength(50)]
        public string ChangeType { get; set; } = string.Empty; // Title, MetaDescription, Keywords, Content
        
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        [StringLength(200)]
        public string Trigger { get; set; } = string.Empty; // Why was it optimized
        
        // Performance before optimization (JSON)
        public string? PerformanceBefore { get; set; }
        
        // Performance after optimization (JSON) - updated after 14 days
        public string? PerformanceAfter { get; set; }
        
        public decimal? RevenueImpact { get; set; }
        public decimal? TrafficImpact { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Applied"; // Applied, Reverted, Testing
        
        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        // Navigation
        public virtual Post Post { get; set; } = null!;
    }
}

