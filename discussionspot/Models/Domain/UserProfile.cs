using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    public class UserProfile : BaseEntity
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [StringLength(2048)]
        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }

        [StringLength(2048)]
        [Display(Name = "Banner URL")]
        public string? BannerUrl { get; set; }

        [StringLength(2048)]
        [Display(Name = "Website")]
        [DataType(DataType.Url)]
        public string? Website { get; set; }

        [StringLength(100)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Join Date")]
        [DataType(DataType.DateTime)]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Active")]
        [DataType(DataType.DateTime)]
        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        [Display(Name = "Karma Points")]
        public int KarmaPoints { get; set; } = 0;

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; } = false;

        // Navigation properties
        public virtual ApplicationUsers User { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public string InitialLetters => string.IsNullOrEmpty(DisplayName) ? "U" :
            DisplayName.Length > 1 ? DisplayName.Substring(0, 2).ToUpper() : DisplayName.Substring(0, 1).ToUpper();
    }
}
