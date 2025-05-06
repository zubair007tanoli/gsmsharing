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
    public class ApplicationDbContext : IdentityDbContext<Models.Domain.ApplicationUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for all models
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Community> Communities { get; set; } = null!;
        public DbSet<CommunityMember> CommunityMembers { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Media> Media { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<PostVote> PostVotes { get; set; } = null!;
        public DbSet<CommentVote> CommentVotes { get; set; } = null!;
        public DbSet<SeoMetadata> SeoMetadata { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<PostTag> PostTags { get; set; } = null!;
        public DbSet<Award> Awards { get; set; } = null!;
        public DbSet<PostAward> PostAwards { get; set; } = null!;
        public DbSet<CommentAward> CommentAwards { get; set; } = null!;
        public DbSet<PollOption> PollOptions { get; set; } = null!;
        public DbSet<PollVote> PollVotes { get; set; } = null!;
        public DbSet<PollConfiguration> PollConfigurations { get; set; } = null!;

        /// <summary>
        /// Configure entity relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed identity roles
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "20250401000000"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "20250401000001"
                },
                new IdentityRole
                {
                    Id = "3",
                    Name = "Editor",
                    NormalizedName = "EDITOR",
                    ConcurrencyStamp = "20250401000002"
                }
            };
            modelBuilder.Entity<IdentityRole>().HasData(roles);

            // Configure UserProfile
            modelBuilder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Category
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            // Configure Community
            modelBuilder.Entity<Community>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Communities)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Community>()
                .HasOne(c => c.Creator)
                .WithMany(u => u.CreatedCommunities)
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Community>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            // Configure CommunityMember
            modelBuilder.Entity<CommunityMember>()
                .HasKey(cm => new { cm.UserId, cm.CommunityId });

            modelBuilder.Entity<CommunityMember>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.CommunityMemberships)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunityMember>()
                .HasOne(cm => cm.Community)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Community)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasIndex(p => new { p.Slug, p.CommunityId })
                .IsUnique();

            // Configure Media
            modelBuilder.Entity<Media>()
                .HasOne(m => m.User)
                .WithMany(u => u.MediaUploads)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Media>()
                .HasOne(m => m.Post)
                .WithMany(p => p.Media)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.ChildComments)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PostVote
            modelBuilder.Entity<PostVote>()
                .HasKey(pv => new { pv.UserId, pv.PostId });

            modelBuilder.Entity<PostVote>()
                .HasOne(pv => pv.User)
                .WithMany(u => u.PostVotes)
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostVote>()
                .HasOne(pv => pv.Post)
                .WithMany(p => p.Votes)
                .HasForeignKey(pv => pv.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostVote>()
                .HasCheckConstraint("CK_PostVote_Type", "VoteType IN (-1, 1)");

            // Configure CommentVote
            modelBuilder.Entity<CommentVote>()
                .HasKey(cv => new { cv.UserId, cv.CommentId });

            modelBuilder.Entity<CommentVote>()
                .HasOne(cv => cv.User)
                .WithMany(u => u.CommentVotes)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentVote>()
                .HasOne(cv => cv.Comment)
                .WithMany(c => c.Votes)
                .HasForeignKey(cv => cv.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentVote>()
                .HasCheckConstraint("CK_CommentVote_Type", "VoteType IN (-1, 1)");

            // Configure SeoMetadata
            modelBuilder.Entity<SeoMetadata>()
                .HasKey(sm => new { sm.EntityType, sm.EntityId });

            modelBuilder.Entity<SeoMetadata>()
                .HasCheckConstraint("CK_SeoMetadata_EntityType", "EntityType IN ('community', 'post')");

            // Configure Tag
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Slug)
                .IsUnique();

            // Configure PostTag
            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Award
            modelBuilder.Entity<Award>()
                .HasIndex(a => a.Name)
                .IsUnique();

            // Configure PostAward
            modelBuilder.Entity<PostAward>()
                .HasOne(pa => pa.Post)
                .WithMany(p => p.Awards)
                .HasForeignKey(pa => pa.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostAward>()
                .HasOne(pa => pa.Award)
                .WithMany(a => a.PostAwards)
                .HasForeignKey(pa => pa.AwardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostAward>()
                .HasOne(pa => pa.AwardedByUser)
                .WithMany(u => u.GivenPostAwards)
                .HasForeignKey(pa => pa.AwardedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CommentAward
            modelBuilder.Entity<CommentAward>()
                .HasOne(ca => ca.Comment)
                .WithMany(c => c.Awards)
                .HasForeignKey(ca => ca.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentAward>()
                .HasOne(ca => ca.Award)
                .WithMany(a => a.CommentAwards)
                .HasForeignKey(ca => ca.AwardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentAward>()
                .HasOne(ca => ca.AwardedByUser)
                .WithMany(u => u.GivenCommentAwards)
                .HasForeignKey(ca => ca.AwardedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure PollOption
            modelBuilder.Entity<PollOption>()
                .HasOne(po => po.Post)
                .WithMany(p => p.PollOptions)
                .HasForeignKey(po => po.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PollVote
            modelBuilder.Entity<PollVote>()
                .HasKey(pv => new { pv.UserId, pv.PollOptionId });

            modelBuilder.Entity<PollVote>()
                .HasOne(pv => pv.User)
                .WithMany(u => u.PollVotes)
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollVote>()
                .HasOne(pv => pv.PollOption)
                .WithMany(po => po.Votes)
                .HasForeignKey(pv => pv.PollOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PollConfiguration
            modelBuilder.Entity<PollConfiguration>()
                .HasOne(pc => pc.Post)
                .WithOne(p => p.PollConfiguration)
                .HasForeignKey<PollConfiguration>(pc => pc.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollConfiguration>()
                .HasCheckConstraint("CK_PollConfiguration_MinOptions", "MinOptions >= 2");

            modelBuilder.Entity<PollConfiguration>()
                .HasCheckConstraint("CK_PollConfiguration_MaxOptions", "MaxOptions >= MinOptions");

            // Configure Community Type constraint
            modelBuilder.Entity<Community>()
                .HasCheckConstraint("CK_Community_Type", "CommunityType IN ('public', 'private', 'restricted')");

            // Create indexes for better performance
            modelBuilder.Entity<Post>()
                .HasIndex(p => new { p.CommunityId, p.CreatedAt });

            modelBuilder.Entity<Post>()
                .HasIndex(p => new { p.UserId, p.CreatedAt });

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.Slug);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.PostId, c.CreatedAt });

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.ParentCommentId);

            modelBuilder.Entity<Media>()
                .HasIndex(m => m.PostId);

            modelBuilder.Entity<Media>()
                .HasIndex(m => m.UserId);

            modelBuilder.Entity<UserProfile>()
                .HasIndex(p => p.DisplayName);

            modelBuilder.Entity<PostTag>()
                .HasIndex(pt => pt.TagId);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Slug);

            // Poll-specific indexes
            modelBuilder.Entity<PollOption>()
                .HasIndex(po => po.PostId);

            modelBuilder.Entity<PollVote>()
                .HasIndex(pv => pv.PollOptionId);

            modelBuilder.Entity<PollVote>()
                .HasIndex(pv => pv.UserId);

            modelBuilder.Entity<PollConfiguration>()
                .HasIndex(pc => pc.EndDate);

            // Add indexes for posts with polls
            modelBuilder.Entity<Post>()
                .HasIndex(p => new { p.PostType, p.HasPoll })
                .HasFilter("PostType = 'poll' AND HasPoll = 1");

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.PollExpiresAt)
                .HasFilter("PollExpiresAt IS NOT NULL");
        }
    }
}
