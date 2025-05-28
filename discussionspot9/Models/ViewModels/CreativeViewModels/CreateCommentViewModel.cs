using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(10000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 10000 characters")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        // Set by controller
        public string? UserId { get; set; }
    }
}
