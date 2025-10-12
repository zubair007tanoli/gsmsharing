using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// AI-generated content recommendations based on performance
    /// </summary>
    public class ContentRecommendation
    {
        [Key]
        public int Id { get; set; }
        
        [StringLength(100)]
        public string RecommendationType { get; set; } = string.Empty; // NewTopic, UpdatePost, CreateSeries, CrossLink
        
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public int? RelatedPostId { get; set; }
        public int? CommunityId { get; set; }
        
        public decimal EstimatedRevenueImpact { get; set; }
        public decimal EstimatedTrafficImpact { get; set; }
        public decimal ConfidenceScore { get; set; }
        
        public int Priority { get; set; }
        
        // Data backing the recommendation (JSON)
        public string? AnalysisData { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Implemented
        
        public DateTime CreatedAt { get; set; }
        public DateTime? ImplementedAt { get; set; }
        
        public string? ImplementedBy { get; set; }
        
        // Navigation
        public virtual Post? RelatedPost { get; set; }
        public virtual Community? Community { get; set; }
    }
}

