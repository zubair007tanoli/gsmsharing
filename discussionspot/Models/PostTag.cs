using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PostTag
    {
        public int PostId { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
