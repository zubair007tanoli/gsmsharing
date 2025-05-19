namespace discussionspot.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string BannerUrl { get; set; }
        public string Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public int KarmaPoints { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsModerator { get; set; }

        // Helper for generating initials for avatar placeholder
        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return "U";

                var parts = DisplayName.Split(' ');
                if (parts.Length == 1)
                    return parts[0].Substring(0, 1).ToUpper();

                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
            }
        }
    }
}
