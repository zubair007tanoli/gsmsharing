using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class SavedPost
    {
        public int SavedPostId { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public DateTime SavedAt { get; set; }

        public virtual Post Post { get; set; }
        public virtual IdentityUser User { get; set; }
    }
}
