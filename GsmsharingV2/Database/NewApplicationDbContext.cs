using GsmsharingV2.Models.NewSchema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Database
{
    // New database context for gsmsharingv4 schema
    // This uses BIGINT for Identity IDs
    public class NewApplicationDbContext : IdentityDbContext<IdentityUser<long>, IdentityRole<long>, long>
    {
        public NewApplicationDbContext(DbContextOptions<NewApplicationDbContext> options) : base(options) { }

        // Classified Ads
        public DbSet<AdCategory> AdCategories { get; set; }
        public DbSet<ClassifiedAd> ClassifiedAds { get; set; }
        public DbSet<AdImage> AdImages { get; set; }
        public DbSet<SavedAd> SavedAds { get; set; }

        // Chat/Messaging
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        // File Repository
        public DbSet<FileCategory> FileCategories { get; set; }
        public DbSet<FileRepository> FileRepositories { get; set; }

        // Affiliate Marketing
        public DbSet<AffiliatePartner> AffiliatePartners { get; set; }
        public DbSet<AffiliateProductNew> AffiliateProducts { get; set; }
        public DbSet<AffiliateClick> AffiliateClicks { get; set; }

        // Posts & Communities
        public DbSet<Community> Communities { get; set; }
        public DbSet<Post> Posts { get; set; }

        // System
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity to use BIGINT
            builder.Entity<IdentityUser<long>>().ToTable("AspNetUsers");
            builder.Entity<IdentityRole<long>>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<long>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<long>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("AspNetRoleClaims");
            builder.Entity<IdentityUserLogin<long>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<long>>().ToTable("AspNetUserTokens");

            // Configure ClassifiedAds
            builder.Entity<ClassifiedAd>(entity =>
            {
                entity.ToTable("ClassifiedAds");
                entity.HasKey(e => e.AdID);
                entity.Property(e => e.AdID).UseIdentityColumn();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.CategoryID);
                entity.HasIndex(e => e.UserID);
                entity.HasIndex(e => e.Status);
            });

            // Configure AdImages
            builder.Entity<AdImage>(entity =>
            {
                entity.ToTable("AdImages");
                entity.HasKey(e => e.ImageID);
                entity.Property(e => e.ImageID).UseIdentityColumn();
                entity.HasIndex(e => e.AdID);
            });

            // Configure ChatConversations
            builder.Entity<ChatConversation>(entity =>
            {
                entity.ToTable("ChatConversations");
                entity.HasKey(e => e.ConversationID);
                entity.Property(e => e.ConversationID).UseIdentityColumn();
                entity.HasIndex(e => new { e.BuyerID, e.SellerID });
            });

            // Configure ChatMessages
            builder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("ChatMessages");
                entity.HasKey(e => e.MessageID);
                entity.Property(e => e.MessageID).UseIdentityColumn();
                entity.HasIndex(e => e.ConversationID);
            });

            // Configure FileRepository
            builder.Entity<FileRepository>(entity =>
            {
                entity.ToTable("FileRepository");
                entity.HasKey(e => e.FileID);
                entity.Property(e => e.FileID).UseIdentityColumn();
                entity.Property(e => e.PriceDisplay).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.CategoryID);
                entity.HasIndex(e => e.MD5Checksum);
            });

            // Configure AffiliateProducts
            builder.Entity<AffiliateProductNew>(entity =>
            {
                entity.ToTable("AffiliateProducts");
                entity.HasKey(e => e.ProductID);
                entity.Property(e => e.ProductID).UseIdentityColumn();
                entity.Property(e => e.PriceDisplay).HasColumnType("decimal(18,2)");
            });

            // Configure AffiliateClicks
            builder.Entity<AffiliateClick>(entity =>
            {
                entity.ToTable("AffiliateClicks");
                entity.HasKey(e => e.ClickID);
                entity.Property(e => e.ClickID).UseIdentityColumn();
                entity.HasIndex(e => e.ClickDate);
            });

            // Configure SystemSettings
            builder.Entity<SystemSetting>(entity =>
            {
                entity.ToTable("SystemSettings");
                entity.HasKey(e => e.SettingKey);
            });

            // Configure AdminLogs
            builder.Entity<AdminLog>(entity =>
            {
                entity.ToTable("AdminLogs");
                entity.HasKey(e => e.LogID);
                entity.Property(e => e.LogID).UseIdentityColumn();
            });

            // Configure Communities
            builder.Entity<Community>(entity =>
            {
                entity.ToTable("Communities");
                entity.HasKey(e => e.CommunityID);
                entity.Property(e => e.CommunityID).UseIdentityColumn();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.CreatorID);
            });

            // Configure Posts
            builder.Entity<Post>(entity =>
            {
                entity.ToTable("Posts");
                entity.HasKey(e => e.PostID);
                entity.Property(e => e.PostID).UseIdentityColumn();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CommunityID);
                entity.HasIndex(e => e.Slug);
                entity.HasIndex(e => e.PostStatus);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.IsPromoted);
            });

            // Relationships
            // Note: User relationships are handled via UserID (long) foreign key
            // We don't configure EF relationships to IdentityUser<long> to avoid conflicts

            builder.Entity<ClassifiedAd>()
                .HasOne(c => c.Category)
                .WithMany(c => c.ClassifiedAds)
                .HasForeignKey(c => c.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AdImage>()
                .HasOne(a => a.Ad)
                .WithMany(c => c.Images)
                .HasForeignKey(a => a.AdID)
                .OnDelete(DeleteBehavior.Cascade);

            // SavedAd User relationship via UserID foreign key

            builder.Entity<SavedAd>()
                .HasOne(s => s.Ad)
                .WithMany(a => a.SavedAds)
                .HasForeignKey(s => s.AdID)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatConversation User relationships via BuyerID/SellerID foreign keys

            builder.Entity<ChatConversation>()
                .HasOne(c => c.Ad)
                .WithMany()
                .HasForeignKey(c => c.AdID)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationID)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage User relationship via SenderID foreign key

            builder.Entity<FileCategory>()
                .HasOne(f => f.ParentCategory)
                .WithMany(p => p.SubCategories)
                .HasForeignKey(f => f.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FileRepository>()
                .HasOne(f => f.Category)
                .WithMany(c => c.Files)
                .HasForeignKey(f => f.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AffiliateProductNew>()
                .HasOne(p => p.Partner)
                .WithMany(pr => pr.Products)
                .HasForeignKey(p => p.PartnerID)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AffiliateClick>()
                .HasOne(c => c.Product)
                .WithMany(p => p.ClicksData)
                .HasForeignKey(c => c.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: User relationships are handled via foreign keys (UserID, BuyerID, SellerID, etc.)
            // We don't configure EF relationships to IdentityUser<long> to avoid Identity conflicts
            // To load user data, query IdentityUser<long> separately using the UserID
        }
    }
}

