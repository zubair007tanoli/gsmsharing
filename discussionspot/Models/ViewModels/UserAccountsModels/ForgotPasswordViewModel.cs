using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.ViewModels.UserAccountsModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
