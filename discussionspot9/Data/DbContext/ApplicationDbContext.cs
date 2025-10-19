using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Data.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityMember> CommunityMembers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostVote> PostVotes { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<SeoMetadata> SeoMetadata { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Award> Awards { get; set; }
        public DbSet<PostAward> PostAwards { get; set; }
        public DbSet<CommentAward> CommentAwards { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<PollConfiguration> PollConfigurations { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<CommentLinkPreview> CommentLinkPreviews { get; set; }
        
        // SEO & Analytics tables
        public DbSet<PostPerformanceMetric> PostPerformanceMetrics { get; set; }
        public DbSet<SeoOptimizationLog> SeoOptimizationLogs { get; set; }
        public DbSet<PostSeoQueue> PostSeoQueues { get; set; }
        public DbSet<AdSenseRevenue> AdSenseRevenues { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<ContentRecommendation> ContentRecommendations { get; set; }
        
        // Enhanced SEO & Multi-Site Revenue tables
        public DbSet<PostKeyword> PostKeywords { get; set; }
        public DbSet<EnhancedSeoMetadata> EnhancedSeoMetadata { get; set; }
        public DbSet<MultiSiteRevenue> MultiSiteRevenues { get; set; }
        
        // Chat System tables
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }
        public DbSet<UserPresence> UserPresences { get; set; }
        public DbSet<ChatAd> ChatAds { get; set; }
        
        // Admin & Moderation System tables
        public DbSet<UserBan> UserBans { get; set; }
        public DbSet<ModerationLog> ModerationLogs { get; set; }
        public DbSet<SiteRole> SiteRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region UserProfile Configuration
            builder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Bio).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.AvatarUrl).HasMaxLength(2048);
                entity.Property(e => e.BannerUrl).HasMaxLength(2048);
                entity.Property(e => e.Website).HasMaxLength(2048);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.JoinDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LastActive).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.KarmaPoints).HasDefaultValue(0);
                entity.Property(e => e.IsVerified).HasDefaultValue(false);

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<UserProfile>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region Category Configuration
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.ParentCategoryId);

                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.ChildCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Community Configuration
            builder.Entity<Community>(entity =>
            {
                entity.HasKey(e => e.CommunityId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.ShortDescription).HasMaxLength(500);
                entity.Property(e => e.CreatorId).HasMaxLength(450);
                entity.Property(e => e.CommunityType).HasMaxLength(20).HasDefaultValue("public");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IconUrl).HasMaxLength(2048);
                entity.Property(e => e.BannerUrl).HasMaxLength(2048);
                entity.Property(e => e.ThemeColor).HasMaxLength(20);
                entity.Property(e => e.MemberCount).HasDefaultValue(0);
                entity.Property(e => e.PostCount).HasDefaultValue(0);
                entity.Property(e => e.Rules).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.IsNSFW).HasDefaultValue(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.CategoryId);

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Communities)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Community_Type", "CommunityType IN ('public', 'private', 'restricted')");
                });
            });
            #endregion

            #region CommunityMember Configuration
            builder.Entity<CommunityMember>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CommunityId });
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("member");
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.NotificationPreference).HasMaxLength(20).HasDefaultValue("all");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Community)
                    .WithMany(e => e.Members)
                    .HasForeignKey(e => e.CommunityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t => { t.HasCheckConstraint("CK_CommunityMember_Role", "Role IN ('member', 'moderator', 'admin')"); });
                entity.ToTable(t => { t.HasCheckConstraint("CK_CommunityMember_NotificationPreference", "NotificationPreference IN ('all', 'important', 'none')"); });
            });
            #endregion

            #region Post Configuration
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.PostId);
                entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(320).IsRequired();
                entity.Property(e => e.Content).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.PostType).HasMaxLength(20).HasDefaultValue("text");
                entity.Property(e => e.Url).HasMaxLength(2048);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpvoteCount).HasDefaultValue(0);
                entity.Property(e => e.DownvoteCount).HasDefaultValue(0);
                entity.Property(e => e.CommentCount).HasDefaultValue(0);
                entity.Property(e => e.Score).HasDefaultValue(0);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("published");
                entity.Property(e => e.IsPinned).HasDefaultValue(false);
                entity.Property(e => e.IsLocked).HasDefaultValue(false);
                entity.Property(e => e.IsNSFW).HasDefaultValue(false);
                entity.Property(e => e.IsSpoiler).HasDefaultValue(false);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.HasPoll).HasDefaultValue(false);
                entity.Property(e => e.PollOptionCount).HasDefaultValue(0);
                entity.Property(e => e.PollVoteCount).HasDefaultValue(0);

                entity.HasIndex(e => new { e.Slug, e.CommunityId }).IsUnique();
                entity.HasIndex(e => new { e.CommunityId, e.CreatedAt });
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => e.Slug);
                entity.HasIndex(e => new { e.PostType, e.HasPoll }).HasFilter("PostType = 'poll' AND HasPoll = 1");
                entity.HasIndex(e => e.PollExpiresAt).HasFilter("PollExpiresAt IS NOT NULL");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Community)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.CommunityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.UserProfile)
                       .WithMany()
                       .HasForeignKey(p => p.UserId)
                       .OnDelete(DeleteBehavior.SetNull);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Post_Type", "PostType IN ('text', 'link', 'image', 'video', 'poll')");
                    t.HasCheckConstraint("CK_Post_Status", "Status IN ('published', 'removed', 'deleted', 'archived')");
                });
            });
            #endregion

            #region Media Configuration
            builder.Entity<Media>(entity =>
            {
                entity.HasKey(e => e.MediaId);
                entity.Property(e => e.Url).HasMaxLength(2048).IsRequired();
                entity.Property(e => e.ThumbnailUrl).HasMaxLength(2048);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.MediaType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.Caption).HasMaxLength(500);
                entity.Property(e => e.AltText).HasMaxLength(500);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.StorageProvider).HasMaxLength(50).HasDefaultValue("local");
                entity.Property(e => e.StoragePath).HasMaxLength(500);
                entity.Property(e => e.IsProcessed).HasDefaultValue(false);

                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Media)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t => { t.HasCheckConstraint("CK_Media_Type", "MediaType IN ('image', 'video', 'document', 'audio')"); });
            });
            #endregion

            #region Comment Configuration
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.Content).HasColumnType("NVARCHAR(MAX)").IsRequired();
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpvoteCount).HasDefaultValue(0);
                entity.Property(e => e.DownvoteCount).HasDefaultValue(0);
                entity.Property(e => e.Score).HasDefaultValue(0);
                entity.Property(e => e.IsEdited).HasDefaultValue(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.TreeLevel).HasDefaultValue(0);

                entity.HasIndex(e => new { e.PostId, e.CreatedAt });
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ParentCommentId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentComment)
                    .WithMany(e => e.ChildComments)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region CommentLinkPreview Configuration
            builder.Entity<CommentLinkPreview>(entity =>
            {
                entity.HasKey(e => e.CommentLinkPreviewId);
                entity.Property(e => e.Url).HasMaxLength(2048).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Domain).HasMaxLength(255);
                entity.Property(e => e.ThumbnailUrl).HasMaxLength(2048);
                entity.Property(e => e.FaviconUrl).HasMaxLength(2048);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.FetchSucceeded).HasDefaultValue(true);

                entity.HasIndex(e => e.CommentId);
                entity.HasIndex(e => e.Url); // For caching lookups

                entity.HasOne(e => e.Comment)
                    .WithMany(e => e.LinkPreviews)
                    .HasForeignKey(e => e.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region Vote Configurations
            builder.Entity<PostVote>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.PostId });
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.VotedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Votes)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t => { t.HasCheckConstraint("CK_PostVote_Type", "VoteType IN (-1, 1)"); });
            });

            builder.Entity<CommentVote>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CommentId });
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.VotedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Comment)
                    .WithMany(e => e.Votes)
                    .HasForeignKey(e => e.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_CommentVote_Type", "VoteType IN (-1, 1)");
                });
            });
            #endregion

            #region SEO Configuration
            builder.Entity<SeoMetadata>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_SeoMetadata_EntityType", "EntityType IN ('community', 'post')");
                });

                entity.HasKey(e => new { e.EntityType, e.EntityId });
                entity.Property(e => e.EntityType).HasMaxLength(20);
                entity.Property(e => e.MetaTitle).HasMaxLength(200);
                entity.Property(e => e.MetaDescription).HasMaxLength(500);
                entity.Property(e => e.CanonicalUrl).HasMaxLength(2048);
                entity.Property(e => e.OgTitle).HasMaxLength(200);
                entity.Property(e => e.OgDescription).HasMaxLength(500);
                entity.Property(e => e.OgImageUrl).HasMaxLength(2048);
                entity.Property(e => e.TwitterCard).HasMaxLength(20).HasDefaultValue("summary");
                entity.Property(e => e.TwitterTitle).HasMaxLength(200);
                entity.Property(e => e.TwitterDescription).HasMaxLength(500);
                entity.Property(e => e.TwitterImageUrl).HasMaxLength(2048);
                entity.Property(e => e.Keywords).HasMaxLength(500);
                entity.Property(e => e.StructuredData).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });
            #endregion

            #region Tag Configuration
            builder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.TagId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.PostCount).HasDefaultValue(0);

                entity.HasIndex(e => e.Slug).IsUnique();
            });

            builder.Entity<PostTag>(entity =>
            {
                entity.HasKey(e => new { e.PostId, e.TagId });

                entity.HasIndex(e => e.TagId);

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.PostTags)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(e => e.PostTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region Award Configuration
            builder.Entity<Award>(entity =>
            {
                entity.HasKey(e => e.AwardId);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconUrl).HasMaxLength(2048).IsRequired();
                entity.Property(e => e.GiveKarma).HasDefaultValue(0);
                entity.Property(e => e.ReceiveKarma).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            builder.Entity<PostAward>(entity =>
            {
                entity.HasKey(e => e.PostAwardId);
                entity.Property(e => e.AwardedByUserId).HasMaxLength(450);
                entity.Property(e => e.AwardedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.IsAnonymous).HasDefaultValue(false);

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Awards)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Award)
                    .WithMany(e => e.PostAwards)
                    .HasForeignKey(e => e.AwardId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AwardedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AwardedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<CommentAward>(entity =>
            {
                entity.HasKey(e => e.CommentAwardId);
                entity.Property(e => e.AwardedByUserId).HasMaxLength(450);
                entity.Property(e => e.AwardedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.IsAnonymous).HasDefaultValue(false);

                entity.HasOne(e => e.Comment)
                    .WithMany(e => e.Awards)
                    .HasForeignKey(e => e.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Award)
                    .WithMany(e => e.CommentAwards)
                    .HasForeignKey(e => e.AwardId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AwardedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AwardedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Notification Configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.EntityType).HasMaxLength(50);
                entity.Property(e => e.EntityId).HasMaxLength(450);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.UserId, e.IsRead });

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region Poll Configuration
            builder.Entity<PollOption>(entity =>
            {
                entity.HasKey(e => e.PollOptionId);
                entity.Property(e => e.OptionText).HasMaxLength(255).IsRequired();
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.VoteCount).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.PostId);

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.PollOptions)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PollVote>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.PollOptionId });
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.VotedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.PollOptionId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PollOption)
                    .WithMany(e => e.Votes)
                    .HasForeignKey(e => e.PollOptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PollConfiguration>(entity =>
            {
                entity.HasKey(e => e.PostId);
                
                // Property configurations
                entity.Property(e => e.AllowMultipleChoices).HasDefaultValue(false);
                entity.Property(e => e.ShowResultsBeforeVoting).HasDefaultValue(true);
                entity.Property(e => e.ShowResultsBeforeEnd).HasDefaultValue(true);
                entity.Property(e => e.AllowAddingOptions).HasDefaultValue(false);
                entity.Property(e => e.MinOptions).HasDefaultValue(2);
                entity.Property(e => e.MaxOptions).HasDefaultValue(10);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.PollQuestion).HasMaxLength(500);
                entity.Property(e => e.PollDescription).HasMaxLength(1000);
                entity.Property(e => e.ClosedByUserId).HasMaxLength(450);

                // Indexes
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.ClosedByUserId);

                // Relationships
                entity.HasOne(e => e.Post)
                    .WithOne(e => e.PollConfiguration)
                    .HasForeignKey<PollConfiguration>(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ClosedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ClosedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Check constraints
                entity.ToTable(t => { t.HasCheckConstraint("CK_PollConfiguration_MinOptions", "MinOptions >= 2"); });
                entity.ToTable(t => { t.HasCheckConstraint("CK_PollConfiguration_MaxOptions", "MaxOptions >= MinOptions"); });
            });
            #endregion
            
            #region SavedPost Configuration
            builder.Entity<SavedPost>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.PostId });
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.SavedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region SEO & Analytics Configuration
            
            // PostPerformanceMetric
            builder.Entity<PostPerformanceMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.PostId, e.Date }).IsUnique();
                entity.HasIndex(e => e.Date);
                entity.Property(e => e.BounceRate).HasPrecision(5, 2);
                entity.Property(e => e.SearchCTR).HasPrecision(5, 2);
                entity.Property(e => e.AvgSearchPosition).HasPrecision(5, 2);
                entity.Property(e => e.AdRevenue).HasPrecision(10, 2);
                entity.Property(e => e.AdCTR).HasPrecision(5, 2);
                entity.Property(e => e.CPC).HasPrecision(10, 2);
                entity.Property(e => e.RPM).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SeoOptimizationLog
            builder.Entity<SeoOptimizationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.PostId, e.OptimizedAt });
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.OptimizedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.OldValue).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.NewValue).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.PerformanceBefore).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.PerformanceAfter).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.RevenueImpact).HasPrecision(10, 2);
                entity.Property(e => e.TrafficImpact).HasPrecision(10, 2);
                entity.Property(e => e.ApprovedBy).HasMaxLength(450);

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PostSeoQueue
            builder.Entity<PostSeoQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Status, e.Priority });
                entity.HasIndex(e => e.PostId);
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.SuggestedChanges).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.ErrorMessage).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.EstimatedRevenueImpact).HasPrecision(10, 2);

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AdSenseRevenue
            builder.Entity<AdSenseRevenue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Date, e.PostId });
                entity.HasIndex(e => e.PostId);
                entity.Property(e => e.Earnings).HasPrecision(10, 2);
                entity.Property(e => e.EstimatedEarnings).HasPrecision(10, 2);
                entity.Property(e => e.CTR).HasPrecision(5, 2);
                entity.Property(e => e.CPC).HasPrecision(10, 2);
                entity.Property(e => e.RPM).HasPrecision(10, 2);
                entity.Property(e => e.ActiveViewViewableImpressions).HasPrecision(10, 2);
                entity.Property(e => e.Coverage).HasPrecision(5, 2);
                entity.Property(e => e.SyncedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserActivity
            builder.Entity<UserActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.PostId, e.ActivityAt });
                entity.HasIndex(e => new { e.UserId, e.ActivityAt });
                entity.HasIndex(e => e.SessionId);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.ActivityAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Metadata).HasColumnType("NVARCHAR(MAX)");

                entity.HasOne(e => e.Post)
                    .WithMany()
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Community)
                    .WithMany()
                    .HasForeignKey(e => e.CommunityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ContentRecommendation
            builder.Entity<ContentRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Status, e.Priority });
                entity.HasIndex(e => e.RelatedPostId);
                entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.AnalysisData).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.EstimatedRevenueImpact).HasPrecision(10, 2);
                entity.Property(e => e.EstimatedTrafficImpact).HasPrecision(10, 2);
                entity.Property(e => e.ConfidenceScore).HasPrecision(5, 2);
                entity.Property(e => e.ImplementedBy).HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.RelatedPost)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedPostId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Community)
                    .WithMany()
                    .HasForeignKey(e => e.CommunityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region Seed Roles and Test User
            var adminRoleId = "1a2b3c4d-5e6f-7g8h-9i0j-k1l2m3n4o5p6";
            var moderatorRoleId = "2b3c4d5e-6f7g-8h9i-0j1k-l2m3n4o5p6q7";
            var userRoleId = "3c4d5e6f-7g8h-9i0j-1k2l-m3n4o5p6q7r8";

            // Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "admin-stamp-123"
                },
                new IdentityRole
                {
                    Id = moderatorRoleId,
                    Name = "Moderator",
                    NormalizedName = "MODERATOR",
                    ConcurrencyStamp = "moderator-stamp-456"
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "user-stamp-789"
                }
            );

            #region Chat System Configuration
            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.MessageId);
                entity.Property(e => e.Content).HasMaxLength(4000).IsRequired();
                entity.Property(e => e.SenderId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.ReceiverId).HasMaxLength(450);
                entity.Property(e => e.AttachmentUrl).HasMaxLength(2048);
                entity.Property(e => e.AttachmentType).HasMaxLength(50);
                entity.Property(e => e.SentAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Sender)
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Receiver)
                    .WithMany()
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ChatRoom)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(e => e.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => e.SentAt);
            });

            builder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.ChatRoomId);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatorId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.IconUrl).HasMaxLength(2048);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsPublic).HasDefaultValue(true);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.MemberCount).HasDefaultValue(0);

                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Community)
                    .WithMany()
                    .HasForeignKey(e => e.CommunityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ChatRoomMember>(entity =>
            {
                entity.HasKey(e => e.ChatRoomMemberId);
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("member");
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsMuted).HasDefaultValue(false);

                entity.HasOne(e => e.ChatRoom)
                    .WithMany(r => r.Members)
                    .HasForeignKey(e => e.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ChatRoomId, e.UserId }).IsUnique();
            });

            builder.Entity<UserPresence>(entity =>
            {
                entity.HasKey(e => e.PresenceId);
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.ConnectionId).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("online");
                entity.Property(e => e.CurrentPage).HasMaxLength(500);
                entity.Property(e => e.DeviceInfo).HasMaxLength(200);
                entity.Property(e => e.LastSeen).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsTyping).HasDefaultValue(false);
                entity.Property(e => e.TypingInChatWith).HasMaxLength(450);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ConnectionId);
            });

            builder.Entity<ChatAd>(entity =>
            {
                entity.HasKey(e => e.ChatAdId);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(2048);
                entity.Property(e => e.TargetUrl).HasMaxLength(2048);
                entity.Property(e => e.AdType).HasMaxLength(50).HasDefaultValue("banner");
                entity.Property(e => e.Placement).HasMaxLength(50).HasDefaultValue("chat-list");
                entity.Property(e => e.DisplayFrequency).HasDefaultValue(10);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.ImpressionCount).HasDefaultValue(0);
                entity.Property(e => e.ClickCount).HasDefaultValue(0);
                entity.Property(e => e.TargetAudience).HasMaxLength(2000);
                entity.Property(e => e.MinMessages).HasDefaultValue(0);
            });
            #endregion

            // Seed Test User
            var testUserId = "4d5e6f7g-8h9i-0j1k-2l3m-n4o5p6q7r8s9";
            builder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = testUserId,
                    UserName = "testuser@discussionspot.com",
                    NormalizedUserName = "TESTUSER@DISCUSSIONSPOT.COM",
                    Email = "testuser@discussionspot.com",
                    NormalizedEmail = "TESTUSER@DISCUSSIONSPOT.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAEHxTyIJBGYlvyNvzOvQIQ9Q3irzYhq8kGw/8MkAR1QMJlXrNL61DIyv74aEyFMSHvw==", // Password123!
                    SecurityStamp = "test-security-stamp-abc",
                    ConcurrencyStamp = "test-concurrency-stamp-def",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                }
            );

            // Assign User role to test user
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = userRoleId,
                    UserId = testUserId
                }
            );

            // Seed UserProfile for test user
            builder.Entity<UserProfile>().HasData(
                new UserProfile
                {
                    UserId = testUserId,
                    DisplayName = "TestUser",
                    Bio = "This is a test account for DiscussionSpot.",
                    JoinDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastActive = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    KarmaPoints = 0,
                    IsVerified = false
                }
            );
            #endregion
        }
    }
}