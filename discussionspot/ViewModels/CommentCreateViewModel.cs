using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class CommentCreateViewModel
    {
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        [Display(Name = "Comment")]
        public string Content { get; set; }
    }
}
