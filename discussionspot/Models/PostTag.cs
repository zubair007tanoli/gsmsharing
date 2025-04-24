using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PostTag
    {
        [Key]
        [Column(Order = 0)]
        public int PostId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int TagId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }

        [ForeignKey(nameof(TagId))]
        public virtual Tag Tag { get; set; }
    }
}
