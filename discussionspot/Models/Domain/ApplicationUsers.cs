using discussionspot.Models.Posts;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot.Models.Domain
{
    public class ApplicationUsers : IdentityUser
    {
        // Identity already provides Id, UserName, Email, PhoneNumber, etc.

        [NotMapped]
        public string DisplayName => UserName ?? Email?.Split('@')[0] ?? Id;

        // Navigation properties
        public virtual UserProfile? Profile { get; set; }

        // Post-related navigation properties
        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Media>? MediaUploads { get; set; }

        // Community-related navigation properties
        public virtual ICollection<Community>? CreatedCommunities { get; set; }
        public virtual ICollection<CommunityMember>? CommunityMemberships { get; set; }

        // Voting-related navigation properties
        public virtual ICollection<PostVote>? PostVotes { get; set; }
        public virtual ICollection<CommentVote>? CommentVotes { get; set; }
        public virtual ICollection<PollVote>? PollVotes { get; set; }

        // Award-related navigation properties
        public virtual ICollection<PostAward>? GivenPostAwards { get; set; }
        public virtual ICollection<CommentAward>? GivenCommentAwards { get; set; }
    }
}
