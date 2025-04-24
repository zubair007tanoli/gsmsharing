using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class VoteViewModel
    {
        [Required]
        public string EntityType { get; set; } // "post" or "comment"

        [Required]
        public int EntityId { get; set; }

        [Required]
        public int VoteType { get; set; } // 1 or -1
    }
}
