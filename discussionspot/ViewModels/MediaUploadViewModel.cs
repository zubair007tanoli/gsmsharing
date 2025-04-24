using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class MediaUploadViewModel
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; }

        [StringLength(500)]
        [Display(Name = "Caption")]
        public string Caption { get; set; }

        [StringLength(500)]
        [Display(Name = "Alt Text")]
        public string AltText { get; set; }

        // Optional association with an entity
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
    }
}
