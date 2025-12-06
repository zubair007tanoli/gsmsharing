using GsmsharingV2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Forum Tables
        public DbSet<ForumThread> UsersFourm { get; set; }
        public DbSet<ForumCategory> ForumCategory { get; set; }
        public DbSet<ForumReply> ForumReplys { get; set; }
        public DbSet<ForumComment> FourmComments { get; set; }

        // Marketplace Tables
        public DbSet<MobileAd> MobileAds { get; set; }
        public DbSet<MobilePartAd> MobilePartAds { get; set; }
        public DbSet<AdImage> AdsImage { get; set; }

        // Mobile Specs
        public DbSet<MobileSpecs> MobileSpecs { get; set; }

        // Existing Tables
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tags> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<CommunityMember> CommunityMembers { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Seed roles
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = DateTime.Now.ToString()
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = DateTime.Now.ToString()
                },
                new IdentityRole
                {
                    Name = "Editor",
                    NormalizedName = "EDITOR",
                    ConcurrencyStamp = DateTime.Now.ToString()
                }
            };
            builder.Entity<IdentityRole>().HasData(roles);

            // Configure Forum Entities
            builder.Entity<ForumThread>().ToTable("UsersFourm")
                .HasKey(f => f.UserFourmID);
            builder.Entity<ForumCategory>().ToTable("ForumCategory")
                .HasKey(fc => fc.CategoryId);
            builder.Entity<ForumReply>().ToTable("ForumReplys")
                .HasKey(fr => fr.Id);
            builder.Entity<ForumComment>().ToTable("FourmComments")
                .HasKey(fc => fc.Commentid);

            // Configure Marketplace Entities
            builder.Entity<MobileAd>().ToTable("MobileAds")
                .HasKey(ma => ma.AdsId);
            builder.Entity<MobilePartAd>().ToTable("MobilePartAds")
                .HasKey(mpa => mpa.MobileAdsId);
            builder.Entity<AdImage>().ToTable("AdsImage")
                .HasKey(a => a.SalePicId);

            // Configure Mobile Specs
            builder.Entity<MobileSpecs>().ToTable("MobileSpecs")
                .HasKey(ms => ms.Specid);
            
            // Configure Core Entities with explicit keys and table names
            builder.Entity<Post>().ToTable("Posts").HasKey(p => p.PostID);
            builder.Entity<Comment>().ToTable("Comments").HasKey(c => c.CommentID);
            builder.Entity<Community>().ToTable("Communities").HasKey(c => c.CommunityID);
            builder.Entity<Category>().ToTable("Categories").HasKey(c => c.CategoryID);
            builder.Entity<Tags>().ToTable("Tags").HasKey(t => t.TagID);
            builder.Entity<Reaction>().ToTable("Reactions").HasKey(r => r.ReactionID);
            builder.Entity<UserProfile>().ToTable("UserProfiles").HasKey(up => up.UserProfileID);
            builder.Entity<CommunityMember>().ToTable("CommunityMembers").HasKey(cm => cm.CommunityMemberID);
            builder.Entity<ChatRoom>().ToTable("ChatRooms").HasKey(cr => cr.RoomID);
            builder.Entity<Notification>().ToTable("Notifications").HasKey(n => n.NotificationID);
            
            // Configure Composite Keys with table names
            builder.Entity<PostTag>().ToTable("PostTags").HasKey(pt => new { pt.PostID, pt.TagID });
            builder.Entity<ChatRoomMember>().ToTable("ChatRoomMembers").HasKey(crm => new { crm.RoomID, crm.UserId });

            // Configure relationships
            builder.Entity<ForumThread>()
                .HasMany(f => f.Replies)
                .WithOne(r => r.Thread)
                .HasForeignKey(r => r.ThreadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ForumThread>()
                .HasMany(f => f.Categories)
                .WithOne(c => c.Thread)
                .HasForeignKey(c => c.UserFourmID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MobileAd>()
                .HasMany(a => a.Images)
                .WithOne(i => i.Ad)
                .HasForeignKey(i => i.AdsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

