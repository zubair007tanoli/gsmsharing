using gsmsharing.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace gsmsharing.Database
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
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
        public DbSet<BanAppeal> BanAppeals { get; set; }

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
            builder.Entity<ForumThread>().ToTable("userforum");
            builder.Entity<ForumCategory>().ToTable("ForumCategory");
            builder.Entity<ForumReply>().ToTable("ForumReplys");
            builder.Entity<ForumComment>().ToTable("FourmComments");

            // Configure Marketplace Entities
            builder.Entity<MobileAd>().ToTable("MobileAds");
            builder.Entity<MobilePartAd>().ToTable("MobilePartAds");
            builder.Entity<AdImage>().ToTable("AdsImage");

            // Configure Mobile Specs
            builder.Entity<MobileSpecs>().ToTable("MobileSpecs");

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
