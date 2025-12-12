using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GsmsharingV2.Models
{
    [Table("ProductReview")]
    public class ProductReview
    {
        [Key]
        public int RId { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        public int? BlogId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Review { get; set; }

        public DateTime? ReviewDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("BlogId")]
        public virtual AffiliationProduct? AffiliationProduct { get; set; }
    }
}

