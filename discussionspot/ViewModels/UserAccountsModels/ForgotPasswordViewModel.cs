using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels.UserAccountsModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
