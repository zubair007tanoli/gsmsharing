namespace discussionspot9.Models.ViewModels
{
    public class ConfirmEmailViewModel
    {
        public required string UserId { get; set; }
        public required string Code { get; set; }
        public required string Email { get; set; } // Added Email property  
    }
}
