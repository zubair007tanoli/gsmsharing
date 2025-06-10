using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class ProfileViewModel
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = null!;

        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        [Url]
        [StringLength(2048)]
        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }

        [Url]
        [StringLength(2048)]
        [Display(Name = "Banner URL")]
        public string? BannerUrl { get; set; }

        [Url]
        [StringLength(2048)]
        public string? Website { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Join Date")]
        public DateTime JoinDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Active")]
        public DateTime LastActive { get; set; }

        [Display(Name = "Karma Points")]
        public int KarmaPoints { get; set; }

        [Display(Name = "Verified Account")]
        public bool IsVerified { get; set; }

    }
}
