using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// SEO Score for posts - tracks comprehensive SEO analysis
    /// </summary>
    [Table("SeoScores")]
    public class SeoScore
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        /// <summary>
        /// Overall SEO score (0-100)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal Score { get; set; }
        
        /// <summary>
        /// Score tier: OK (≥80), Needs Improvement (50-79), Critical (<50)
        /// </summary>
        [StringLength(20)]
        public string Tier { get; set; } = "Critical"; // OK, NeedsImprovement, Critical
        
        /// <summary>
        /// Google Search competitiveness score (0-100, 40% weight)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal GoogleCompetitivenessScore { get; set; }
        
        /// <summary>
        /// Content quality score (0-100, 30% weight)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ContentQualityScore { get; set; }
        
        /// <summary>
        /// Meta completeness score (0-100, 15% weight)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal MetaCompletenessScore { get; set; }
        
        /// <summary>
        /// Freshness & engagement score (0-100, 10% weight)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal FreshnessScore { get; set; }
        
        /// <summary>
        /// Image SEO score (0-100, 10% weight) - NEW
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ImageSeoScore { get; set; }
        
        /// <summary>
        /// Technical SEO score (0-100, 10% weight) - NEW
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal TechnicalSeoScore { get; set; }
        
        /// <summary>
        /// Content structure score (0-100, 5% weight) - NEW
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ContentStructureScore { get; set; }
        
        /// <summary>
        /// Top 8 SEO issues (JSON array)
        /// </summary>
        public string? Issues { get; set; }
        
        /// <summary>
        /// Recommended keywords from Google Search (JSON array)
        /// </summary>
        public string? RecommendedKeywords { get; set; }
        
        /// <summary>
        /// Top competitors from Google Search (JSON array)
        /// </summary>
        public string? TopCompetitors { get; set; }
        
        /// <summary>
        /// Priority rank for optimization (higher = more urgent)
        /// </summary>
        public int PriorityRank { get; set; }
        
        /// <summary>
        /// When this score was calculated
        /// </summary>
        public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Source of scoring: Hybrid, GoogleSearch, Python, AI
        /// </summary>
        [StringLength(50)]
        public string Source { get; set; } = "Hybrid";
        
        // Navigation
        public virtual Post Post { get; set; } = null!;
    }
}

