using gsmsharing.Models;

namespace gsmsharing.ViewModels
{
    public class UserProfileViewModel
    {
        public ApplicationUser Users { get; set; }
        public UserProfile Profile { get; set; }
        public UserProfileViewModel()
        {
            Users = new ApplicationUser();
            Profile = new UserProfile();
        }
    }
}
