namespace discussionspot9.Models.ViewModels
{
    public class ConfirmEmailViewModel
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public string Email { get; set; } // Added Email property  
    }
}
