using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GsmsharingV2.Models
{
    [Table("BlogComments")]
    public class BlogComment
    {
        [Key]
        public int Commentid { get; set; }

        public int? BlogId { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string? Comment { get; set; }

        public DateTime? CreationDate { get; set; }

        // Navigation properties
        [ForeignKey("BlogId")]
        public virtual MobilePost? MobilePost { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}

