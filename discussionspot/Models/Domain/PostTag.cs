using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents the many-to-many relationship between posts and tags
    /// </summary>
    public class PostTag
    {
        [Key, Column(Order = 0)]
        public int PostId { get; set; }

        [Key, Column(Order = 1)]
        public int TagId { get; set; }

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }
}
