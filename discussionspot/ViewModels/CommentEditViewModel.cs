using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class CommentEditViewModel
    {
        public int CommentId { get; set; }

        [Required]
        [Display(Name = "Comment")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
