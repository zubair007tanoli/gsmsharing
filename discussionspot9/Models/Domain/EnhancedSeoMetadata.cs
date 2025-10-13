using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Enhanced SEO metadata with click-worthy optimization
    /// </summary>
    [Table("EnhancedSeoMetadata")]
    public class EnhancedSeoMetadata
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        /// <summary>
        /// Original meta description
        /// </summary>
        [MaxLength(500)]
        public string? OriginalMetaDescription { get; set; }
        
        /// <summary>
        /// AI-optimized click-worthy meta description
        /// </summary>
        [MaxLength(500)]
        public string? OptimizedMetaDescription { get; set; }
        
        /// <summary>
        /// Predicted CTR improvement (%)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal PredictedCtrImprovement { get; set; }
        
        /// <summary>
        /// Emotional trigger words used
        /// </summary>
        [MaxLength(500)]
        public string? EmotionalTriggers { get; set; }
        
        /// <summary>
        /// Power words included
        /// </summary>
        [MaxLength(500)]
        public string? PowerWords { get; set; }
        
        /// <summary>
        /// Primary keyword (1-2 keywords)
        /// </summary>
        [MaxLength(200)]
        public string? PrimaryKeywords { get; set; }
        
        /// <summary>
        /// Secondary keywords (3-5 keywords)
        /// </summary>
        [MaxLength(500)]
        public string? SecondaryKeywords { get; set; }
        
        /// <summary>
        /// LSI keywords (5-10 keywords)
        /// </summary>
        [MaxLength(1000)]
        public string? LsiKeywords { get; set; }
        
        /// <summary>
        /// Total keyword search volume
        /// </summary>
        public long TotalSearchVolume { get; set; }
        
        /// <summary>
        /// Readability score (Flesch-Kincaid)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ReadabilityScore { get; set; }
        
        /// <summary>
        /// Overall SEO score (0-100)
        /// </summary>
        public int SeoScore { get; set; }
        
        /// <summary>
        /// Keyword density (%)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal KeywordDensity { get; set; }
        
        /// <summary>
        /// Competitor analysis data (JSON)
        /// </summary>
        public string? CompetitorAnalysis { get; set; }
        
        /// <summary>
        /// SERP preview analysis
        /// </summary>
        public string? SerpPreview { get; set; }
        
        /// <summary>
        /// Whether optimization was manually approved
        /// </summary>
        public bool IsApproved { get; set; }
        
        /// <summary>
        /// Who approved it
        /// </summary>
        [MaxLength(450)]
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public virtual Post? Post { get; set; }
    }
}

