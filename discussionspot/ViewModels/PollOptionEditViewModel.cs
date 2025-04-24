using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class PollOptionEditViewModel
    {
        public int? PollOptionId { get; set; }

        [Required]
        [StringLength(255)]
        public string OptionText { get; set; }

        public int VoteCount { get; set; }

        public bool IsNew { get; set; }

        public bool IsDeleted { get; set; }
    }
}
