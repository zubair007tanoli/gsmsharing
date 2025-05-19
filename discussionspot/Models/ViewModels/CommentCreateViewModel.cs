using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.ViewModels
{
    public class CommentCreateViewModel
    {
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        [StringLength(10000)]
        public string Content { get; set; }
    }
}
