using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsmsharingV2.Models
{
    [Table("BlogSpecContainer")]
    public class BlogSpecContainer
    {
        [Key]
        public int ContainerId { get; set; }

        public int? BlogId { get; set; }

        public int? SpecId { get; set; }

        // Navigation properties
        [ForeignKey("BlogId")]
        public virtual MobilePost? MobilePost { get; set; }

        [ForeignKey("SpecId")]
        public virtual MobileSpecs? MobileSpecs { get; set; }
    }
}

