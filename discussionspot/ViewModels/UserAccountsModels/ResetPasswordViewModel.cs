using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels.UserAccountsModels
{

    public class ResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }

        public string Token { get; set; } // Used for verification
        public string Email { get; set; } // Email to identify the user
    }

}
