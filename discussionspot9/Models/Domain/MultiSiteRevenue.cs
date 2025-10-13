using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Tracks revenue from multiple sites (gsmsharing.com, discussionspot.com)
    /// </summary>
    [Table("MultiSiteRevenues")]
    public class MultiSiteRevenue
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string SiteDomain { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public int? PostId { get; set; } // Null for site-wide stats
        
        [MaxLength(500)]
        public string? PostUrl { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Earnings { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedEarnings { get; set; }
        
        public int PageViews { get; set; }
        public int AdClicks { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal CTR { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal CPC { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal RPM { get; set; }
        
        public int AdImpressions { get; set; }
        public int ActiveViewViewableImpressions { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal Coverage { get; set; }
        
        public DateTime SyncedAt { get; set; }
        
        [MaxLength(50)]
        public string Source { get; set; } = "AdSense";
        
        // Navigation
        public virtual Post? Post { get; set; }
    }
}

