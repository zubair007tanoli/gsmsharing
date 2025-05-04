using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PollOption
    {
        public int PollOptionId { get; set; }
        public int PostId { get; set; }
        public string OptionText { get; set; }
        public int DisplayOrder { get; set; }
        public int VoteCount { get; set; } = 0;
    }
}
