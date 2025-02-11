using System.ComponentModel.DataAnnotations;

namespace gsmsharing.Models
{
    public class SEOModel
    {
       
        public int SEOId { get; set; }

        [Required]
        public int PostID { get; set; }

        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        [MaxLength(500)]
        public string? MetaKeywords { get; set; }

        [MaxLength(200)]
        public string? OgTitle { get; set; }

        [MaxLength(500)]
        public string? OgDescription { get; set; }

        [MaxLength(255)]
        public string? OgImage { get; set; }

        [MaxLength(20)]
        public string? TwitterCard { get; set; }

        [MaxLength(200)]
        public string? TwitterTitle { get; set; }

        [MaxLength(500)]
        public string? TwitterDescription { get; set; }

        [MaxLength(255)]
        public string? TwitterImage { get; set; }

        [MaxLength(500)]
        public string? CanonicalURL { get; set; }

        [MaxLength(50)]
        public string? Robots { get; set; }

        public string? Schema { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
