using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class PostDeleteViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string CommunityName { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Reason for Deletion")]
        public string DeletionReason { get; set; }

        [Display(Name = "Permanently Delete")]
        public bool PermanentDelete { get; set; } = false;
    }
}
