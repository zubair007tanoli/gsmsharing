using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PollOption
    {
        [Key]
        public int PollOptionId { get; set; }

        public int PostId { get; set; }

        [Required]
        [StringLength(255)]
        public string OptionText { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int VoteCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }

        // Navigation properties
        public virtual ICollection<PollVote> Votes { get; set; }
    }
}
