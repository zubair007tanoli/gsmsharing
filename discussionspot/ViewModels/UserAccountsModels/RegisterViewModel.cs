using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels.UserAccountsModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Display name must be between 3 and 100 characters.", MinimumLength = 3)]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "You must agree to the Terms of Service and Privacy Policy")]
        public bool AgreeTerms { get; set; }

        public string ReturnUrl { get; set; } = "/Home/Index";
    }
}