using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class UserProfileEditViewModel
    {
        public string UserId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Display(Name = "Bio")]
        public string Bio { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile AvatarImage { get; set; }

        [Display(Name = "Banner Image")]
        public IFormFile BannerImage { get; set; }

        [Display(Name = "Website")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Website { get; set; }

        [Display(Name = "Location")]
        [StringLength(100)]
        public string Location { get; set; }

        // Existing URLs for display
        public string CurrentAvatarUrl { get; set; }
        public string CurrentBannerUrl { get; set; }
    }
}
