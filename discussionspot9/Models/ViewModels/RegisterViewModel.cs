using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Display Name")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Display name can only contain letters, numbers, underscores, and hyphens")]
        public string DisplayName { get; set; } = null!;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
        [Required]
        [Display(Name = "I agree to the Terms of Service and Privacy Policy")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms and conditions")]
        public bool AgreeToTerms { get; set; }
    }
}
