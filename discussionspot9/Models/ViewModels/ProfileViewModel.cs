using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Display Name")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Display name can only contain letters, numbers, underscores, and hyphens")]
        public string DisplayName { get; set; } = null!;

        [StringLength(500)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [StringLength(100)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Url]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Banner URL")]
        public string? BannerUrl { get; set; }

        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
    }
}
