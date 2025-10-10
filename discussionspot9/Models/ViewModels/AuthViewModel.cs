namespace discussionspot9.Models.ViewModels
{
    public class AuthViewModel
    {
        /// <summary>
        /// Registration model
        /// </summary>
        public RegisterViewModel RegisterModel { get; set; } = new RegisterViewModel()
        {
            AgreeToTerms = false // Default to true for new registrations
        };

        /// <summary>
        /// Login model
        /// </summary>
        public LoginViewModel LoginModel { get; set; } = new LoginViewModel();

        private string? _returnUrl;


        /// <summary>
        /// Return URL after successful authentication
        /// </summary>     

        public string? ReturnUrl
        {
            get => _returnUrl;
            set
            {
                _returnUrl = value;

                // Assign ReturnUrl to both child models if they exist
            
                    RegisterModel.ReturnUrl = value;

             
                    LoginModel.ReturnUrl = value;
            }
        }

        /// <summary>
        /// External login providers available
        /// </summary>
        public List<string>? ExternalLogins { get; set; }

        /// <summary>
        /// Whether to show registration form by default
        /// </summary>
        public bool ShowRegister { get; set; } = false;
    }
}
