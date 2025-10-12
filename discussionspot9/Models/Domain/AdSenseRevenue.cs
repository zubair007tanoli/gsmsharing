using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Tracks AdSense revenue data
    /// </summary>
    public class AdSenseRevenue
    {
        [Key]
        public int Id { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        
        public int? PostId { get; set; } // Null for site-wide stats
        
        // Revenue
        public decimal Earnings { get; set; }
        public decimal EstimatedEarnings { get; set; }
        
        // Performance
        public int PageViews { get; set; }
        public int AdClicks { get; set; }
        public decimal CTR { get; set; }
        public decimal CPC { get; set; }
        public decimal RPM { get; set; }
        public int AdImpressions { get; set; }
        
        // Additional metrics
        public decimal ActiveViewViewableImpressions { get; set; }
        public decimal Coverage { get; set; }
        
        public DateTime SyncedAt { get; set; }
        
        [StringLength(50)]
        public string Source { get; set; } = "AdSense"; // AdSense, Manual
        
        // Navigation
        public virtual Post? Post { get; set; }
    }
}

