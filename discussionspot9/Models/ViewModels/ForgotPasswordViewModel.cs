using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;
    }
}
