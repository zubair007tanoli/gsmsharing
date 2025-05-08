using discussionspot.Models;
using discussionspot.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Data
{
    /// <summary>
    /// The main database context for the application
    /// </summary>

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    namespace discussionspot.Data
    {
        public class ApplicationDbContext : IdentityDbContext<ApplicationUsers>
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            // User related
            public DbSet<UserProfile> UserProfiles { get; set; }

            // Community related
            public DbSet<Community> Communities { get; set; }
            public DbSet<CommunityMember> CommunityMembers { get; set; }
            public DbSet<Category> Categories { get; set; }

            // Post related
            public DbSet<Post> Posts { get; set; }
            public DbSet<PostVote> PostVotes { get; set; }
            public DbSet<PostTag> PostTags { get; set; }
            public DbSet<PostAward> PostAwards { get; set; }

            // Comment related
            public DbSet<Comment> Comments { get; set; }
            public DbSet<CommentVote> CommentVotes { get; set; }
            public DbSet<CommentAward> CommentAwards { get; set; }

            // Poll related
            public DbSet<PollOption> PollOptions { get; set; }
            public DbSet<PollConfiguration> PollConfigurations { get; set; }
            public DbSet<PollVote> PollVotes { get; set; }

            // Media related
            public DbSet<Media> Media { get; set; }

            // Tag related
            public DbSet<Tag> Tags { get; set; }

            // SEO related
            public DbSet<SeoMetadata> SeoMetadata { get; set; }

            // Award related
            public DbSet<Award> Awards { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                // Configure composite keys
                builder.Entity<PostVote>()
                    .HasKey(pv => new { pv.UserId, pv.PostId });

                builder.Entity<CommentVote>()
                    .HasKey(cv => new { cv.UserId, cv.CommentId });

                builder.Entity<PollVote>()
                    .HasKey(pv => new { pv.UserId, pv.PollOptionId });

                builder.Entity<PostTag>()
                    .HasKey(pt => new { pt.PostId, pt.TagId });

                builder.Entity<CommunityMember>()
                    .HasKey(cm => new { cm.UserId, cm.CommunityId });

                builder.Entity<SeoMetadata>()
                    .HasKey(s => new { s.EntityType, s.EntityId });

                // Configure relationships
                builder.Entity<Post>()
                    .HasOne(p => p.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.Entity<Post>()
                    .HasOne(p => p.Community)
                    .WithMany(c => c.Posts)
                    .HasForeignKey(p => p.CommunityId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Comment>()
                    .HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.Entity<Comment>()
                    .HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Comment>()
                    .HasOne(c => c.ParentComment)
                    .WithMany(c => c.ChildComments)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.Entity<Media>()
                    .HasOne(m => m.User)
                    .WithMany(u => u.MediaUploads)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.Entity<Media>()
                    .HasOne(m => m.Post)
                    .WithMany(p => p.Media)
                    .HasForeignKey(m => m.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Community>()
                    .HasOne(c => c.Creator)
                    .WithMany(u => u.CreatedCommunities)
                    .HasForeignKey(c => c.CreatorId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.Entity<Community>()
                    .HasOne(c => c.Category)
                    .WithMany(c => c.Communities)
                    .HasForeignKey(c => c.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Configure indexes
                builder.Entity<Post>()
                    .HasIndex(p => new { p.CommunityId, p.CreatedAt });

                builder.Entity<Post>()
                    .HasIndex(p => p.UserId);

                builder.Entity<Post>()
                    .HasIndex(p => p.Slug);

                builder.Entity<Comment>()
                    .HasIndex(c => c.PostId);

                builder.Entity<Comment>()
                    .HasIndex(c => c.UserId);

                builder.Entity<Community>()
                    .HasIndex(c => c.Slug)
                    .IsUnique();

                builder.Entity<Category>()
                    .HasIndex(c => c.Slug)
                    .IsUnique();

                builder.Entity<Tag>()
                    .HasIndex(t => t.Slug)
                    .IsUnique();

                // Configure enums and other constraints
                builder.Entity<Post>()
                    .Property(p => p.Status)
                    .HasDefaultValue("published");

                builder.Entity<Post>()
                    .Property(p => p.PostType)
                    .HasDefaultValue("text");

                builder.Entity<Community>()
                    .Property(c => c.CommunityType)
                    .HasDefaultValue("public");

                builder.Entity<CommunityMember>()
                    .Property(cm => cm.Role)
                    .HasDefaultValue("member");

                builder.Entity<CommunityMember>()
                    .Property(cm => cm.NotificationPreference)
                    .HasDefaultValue("all");
            }
        }
    }
}
