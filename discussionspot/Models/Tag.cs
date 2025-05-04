using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }
}
