using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsmsharingV2.Models
{
    [Table("BlogSEO")]
    public class BlogSEO
    {
        [Key]
        public int SEOID { get; set; }

        public int? BlogId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? BlogDiscription { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? BlogKeywords { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? canonical { get; set; }

        // Navigation property
        [ForeignKey("BlogId")]
        public virtual GsmBlog? GsmBlog { get; set; }
    }
}

