using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Tracks daily performance metrics for posts
    /// </summary>
    public class PostPerformanceMetric
    {
        [Key]
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        
        // Traffic metrics
        public int Views { get; set; }
        public int UniqueVisitors { get; set; }
        public decimal BounceRate { get; set; }
        public int AvgTimeOnPage { get; set; } // in seconds
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        
        // Search metrics (from Google Search Console)
        public int SearchImpressions { get; set; }
        public int SearchClicks { get; set; }
        public decimal SearchCTR { get; set; }
        public decimal AvgSearchPosition { get; set; }
        
        // Revenue metrics (from AdSense)
        public decimal AdRevenue { get; set; }
        public int AdClicks { get; set; }
        public decimal AdCTR { get; set; }
        public decimal CPC { get; set; } // Cost per click
        public decimal RPM { get; set; } // Revenue per 1000 views
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation
        public virtual Post Post { get; set; } = null!;
    }
}

