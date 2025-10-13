using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Stores keywords for posts with search volume and classification
    /// </summary>
    [Table("PostKeywords")]
    public class PostKeyword
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Keyword { get; set; } = string.Empty;
        
        /// <summary>
        /// Primary, Secondary, LSI (Latent Semantic Indexing)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string KeywordType { get; set; } = "Secondary";
        
        /// <summary>
        /// Monthly search volume from Google Keyword Planner
        /// </summary>
        public long SearchVolume { get; set; }
        
        /// <summary>
        /// Competition level: Low, Medium, High
        /// </summary>
        [MaxLength(20)]
        public string Competition { get; set; } = "Unknown";
        
        /// <summary>
        /// Suggested bid range (CPC)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal SuggestedBidLow { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SuggestedBidHigh { get; set; }
        
        /// <summary>
        /// Keyword difficulty score (0-100)
        /// </summary>
        public int DifficultyScore { get; set; }
        
        /// <summary>
        /// Position in content priority
        /// </summary>
        public int Priority { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public virtual Post? Post { get; set; }
    }
}

