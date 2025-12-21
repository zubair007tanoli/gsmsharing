using Microsoft.AspNetCore.Identity;

namespace GsmsharingV2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarPath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? City { get; set; }
        
        // Navigation properties
        public UserProfile? UserProfile { get; set; }
    }
}

