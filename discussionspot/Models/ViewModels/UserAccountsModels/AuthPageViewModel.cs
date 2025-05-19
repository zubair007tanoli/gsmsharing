namespace discussionspot.Models.ViewModels.UserAccountsModels
{
    public class AuthPageViewModel
    {
        public LoginViewModel LoginModel { get; set; } = new LoginViewModel();
        public RegisterViewModel RegisterModel { get; set; } = new RegisterViewModel();
        public string ReturnUrl { get; set; } = "/Home/Index";
    }
}