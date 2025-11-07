using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// SEO Optimization Proposal - requires approval before applying
    /// </summary>
    [Table("SeoOptimizationProposals")]
    public class SeoOptimizationProposal
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        /// <summary>
        /// Proposed optimized title
        /// </summary>
        [MaxLength(500)]
        public string? ProposedTitle { get; set; }
        
        /// <summary>
        /// Proposed optimized content
        /// </summary>
        public string? ProposedContent { get; set; }
        
        /// <summary>
        /// Proposed meta description
        /// </summary>
        [MaxLength(500)]
        public string? ProposedMetaDescription { get; set; }
        
        /// <summary>
        /// Proposed keywords (comma-separated)
        /// </summary>
        [MaxLength(1000)]
        public string? ProposedKeywords { get; set; }
        
        /// <summary>
        /// Differences/changes summary (JSON)
        /// </summary>
        public string? ChangesSummary { get; set; }
        
        /// <summary>
        /// Expected score improvement
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ExpectedScoreDelta { get; set; }
        
        /// <summary>
        /// Current score before optimization
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal CurrentScore { get; set; }
        
        /// <summary>
        /// Expected score after optimization
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ExpectedScore { get; set; }
        
        /// <summary>
        /// Status: Pending, Approved, Rejected, Applied
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Who created this proposal
        /// </summary>
        [MaxLength(450)]
        public string? CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Who approved/rejected this proposal
        /// </summary>
        [MaxLength(450)]
        public string? ReviewedBy { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        
        /// <summary>
        /// When proposal was applied to post
        /// </summary>
        public DateTime? AppliedAt { get; set; }
        
        /// <summary>
        /// Rejection reason (if rejected)
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }
        
        /// <summary>
        /// Source of optimization: Hybrid, GoogleSearch, AI, etc.
        /// </summary>
        [StringLength(50)]
        public string Source { get; set; } = "Hybrid";
        
        // Navigation
        public virtual Post Post { get; set; } = null!;
    }
}

