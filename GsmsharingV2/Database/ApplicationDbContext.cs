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

        // Blog Tables (from existing database)
        public DbSet<MobilePost> MobilePosts { get; set; }
        public DbSet<GsmBlog> GsmBlogs { get; set; }
        public DbSet<AffiliationProduct> AffiliationProducts { get; set; }
        
        // Blog Related Tables (existing data)
        public DbSet<BlogSEO> BlogSEO { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<ProductReview> ProductReview { get; set; }
        public DbSet<BlogSpecContainer> BlogSpecContainer { get; set; }

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
        
        // New modern tables
        public DbSet<PostVote> PostVotes { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<ForumVote> ForumVotes { get; set; }
        public DbSet<PostReport> PostReports { get; set; }
        public DbSet<CommentReport> CommentReports { get; set; }
        public DbSet<UserBlock> UserBlocks { get; set; }
        public DbSet<SocialShare> SocialShares { get; set; }
        public DbSet<PostView> PostViews { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<PostHistory> PostHistory { get; set; }

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
            builder.Entity<ForumThread>().ToTable("userforum", "gsmsharing")
                .HasKey(f => f.UserFourmID);
            
            // Map ForumThread column names (database uses lowercase)
            builder.Entity<ForumThread>()
                .Property(f => f.Views).HasColumnName("views");
            builder.Entity<ForumThread>()
                .Property(f => f.Likes).HasColumnName("likes");
            builder.Entity<ForumThread>()
                .Property(f => f.Dislikes).HasColumnName("dislikes");
            builder.Entity<ForumThread>()
                .Property(f => f.Publish).HasColumnName("publish");
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

            // Configure Blog Tables
            builder.Entity<MobilePost>().ToTable("MobilePosts")
                .HasKey(mp => mp.BlogId);
            builder.Entity<GsmBlog>().ToTable("GsmBlog")
                .HasKey(gb => gb.BlogId);
            builder.Entity<AffiliationProduct>().ToTable("AffiliationProgram")
                .HasKey(ap => ap.ProductId);
            
            // Configure Blog Related Tables
            builder.Entity<BlogSEO>().ToTable("BlogSEO")
                .HasKey(bs => bs.SEOID);
            builder.Entity<BlogComment>().ToTable("BlogComments")
                .HasKey(bc => bc.Commentid);
            builder.Entity<ProductReview>().ToTable("ProductReview")
                .HasKey(pr => pr.RId);
            builder.Entity<BlogSpecContainer>().ToTable("BlogSpecContainer")
                .HasKey(bsc => bsc.ContainerId);

            // Configure Blog Relationships
            builder.Entity<MobilePost>()
                .HasOne(mp => mp.User)
                .WithMany()
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<GsmBlog>()
                .HasOne(gb => gb.User)
                .WithMany()
                .HasForeignKey(gb => gb.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AffiliationProduct>()
                .HasOne(ap => ap.User)
                .WithMany()
                .HasForeignKey(ap => ap.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure BlogSEO Relationship
            builder.Entity<BlogSEO>()
                .HasOne(bs => bs.GsmBlog)
                .WithMany(gb => gb.BlogSEO)
                .HasForeignKey(bs => bs.BlogId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure BlogComment Relationships
            builder.Entity<BlogComment>()
                .HasOne(bc => bc.MobilePost)
                .WithMany(mp => mp.BlogComments)
                .HasForeignKey(bc => bc.BlogId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.Entity<BlogComment>()
                .HasOne(bc => bc.User)
                .WithMany()
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure ProductReview Relationships
            builder.Entity<ProductReview>()
                .HasOne(pr => pr.User)
                .WithMany()
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.Entity<ProductReview>()
                .HasOne(pr => pr.AffiliationProduct)
                .WithMany(ap => ap.ProductReviews)
                .HasForeignKey(pr => pr.BlogId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure BlogSpecContainer Relationships
            builder.Entity<BlogSpecContainer>()
                .HasOne(bsc => bsc.MobilePost)
                .WithMany(mp => mp.BlogSpecContainers)
                .HasForeignKey(bsc => bsc.BlogId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.Entity<BlogSpecContainer>()
                .HasOne(bsc => bsc.MobileSpecs)
                .WithMany()
                .HasForeignKey(bsc => bsc.SpecId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configure Core Entities with explicit keys and table names
            builder.Entity<Post>().ToTable("Posts").HasKey(p => p.PostID);
            builder.Entity<Comment>().ToTable("Comments").HasKey(c => c.CommentID);
            builder.Entity<Community>().ToTable("Communities").HasKey(c => c.CommunityID);
            builder.Entity<Category>().ToTable("Categories").HasKey(c => c.CategoryID);
            
            // Temporarily ignore new properties that may not exist in database yet
            // Remove these .Ignore() calls after running db_modernized_fixes.sql
            builder.Entity<Post>().Ignore(p => p.IsDeleted);
            builder.Entity<Post>().Ignore(p => p.CanonicalUrl);
            builder.Entity<Post>().Ignore(p => p.FocusKeyword);
            builder.Entity<Post>().Ignore(p => p.Excerpt);
            builder.Entity<Post>().Ignore(p => p.Score);
            builder.Entity<Post>().Ignore(p => p.CommentCount);
            builder.Entity<Post>().Ignore(p => p.UpvoteCount);
            builder.Entity<Post>().Ignore(p => p.DownvoteCount);
            builder.Entity<Post>().Ignore(p => p.IsLocked);
            builder.Entity<Post>().Ignore(p => p.IsPinned);
            builder.Entity<Post>().Ignore(p => p.DeletedAt);
            builder.Entity<Post>().Ignore(p => p.SchemaMarkup);
            // Temporarily ignore MetaTitle and MetaDescription if they don't exist in your database
            // Remove these ignores after ensuring columns exist or after running db_modernized_fixes.sql
            // builder.Entity<Post>().Ignore(p => p.MetaTitle);
            // builder.Entity<Post>().Ignore(p => p.MetaDescription);
            
            builder.Entity<Comment>().Ignore(c => c.IsDeleted);
            builder.Entity<Comment>().Ignore(c => c.UpvoteCount);
            builder.Entity<Comment>().Ignore(c => c.DownvoteCount);
            builder.Entity<Comment>().Ignore(c => c.IsEdited);
            builder.Entity<Comment>().Ignore(c => c.EditedAt);
            builder.Entity<Comment>().Ignore(c => c.DeletedAt);
            
            builder.Entity<Community>().Ignore(c => c.IsDeleted);
            // Temporarily ignore MetaTitle and MetaDescription if they don't exist in your database
            // Remove these ignores after ensuring columns exist or after running db_modernized_fixes.sql
            builder.Entity<Community>().Ignore(c => c.MetaTitle);
            builder.Entity<Community>().Ignore(c => c.MetaDescription);
            builder.Entity<Tags>().ToTable("Tags").HasKey(t => t.TagID);
            builder.Entity<Reaction>().ToTable("Reactions").HasKey(r => r.ReactionID);
            builder.Entity<UserProfile>().ToTable("UserProfiles").HasKey(up => up.UserProfileID);
            builder.Entity<CommunityMember>().ToTable("CommunityMembers").HasKey(cm => cm.CommunityMemberID);
            builder.Entity<ChatRoom>().ToTable("ChatRooms").HasKey(cr => cr.RoomID);
            builder.Entity<Notification>().ToTable("Notifications").HasKey(n => n.NotificationID);
            
            // Configure Composite Keys with table names
            builder.Entity<PostTag>().ToTable("PostTags").HasKey(pt => new { pt.PostID, pt.TagID });
            builder.Entity<ChatRoomMember>().ToTable("ChatRoomMembers").HasKey(crm => new { crm.RoomID, crm.UserId });

            // =============================================
            // Configure Core Entity Relationships
            // =============================================

            // Post Relationships
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Post>()
                .HasOne(p => p.Community)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CommunityID)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Post>()
                .HasMany(p => p.Reactions)
                .WithOne()
                .HasForeignKey(r => r.PostID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Post>()
                .HasMany(p => p.PostTags)
                .WithOne()
                .HasForeignKey(pt => pt.PostID)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment Relationships
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany()
                .HasForeignKey(c => c.ParentCommentID)
                .OnDelete(DeleteBehavior.NoAction);

            // Community Relationships
            builder.Entity<Community>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Community>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Community>()
                .HasMany(c => c.Members)
                .WithOne()
                .HasForeignKey(cm => cm.CommunityID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Community>()
                .HasMany(c => c.ChatRooms)
                .WithOne()
                .HasForeignKey(cr => cr.CommunityID)
                .OnDelete(DeleteBehavior.SetNull);

            // Category Self-Reference
            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Category>()
                .HasMany(c => c.Communities)
                .WithOne(com => com.Category)
                .HasForeignKey(com => com.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            // UserProfile Relationship
            builder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommunityMember Relationships
            builder.Entity<CommunityMember>()
                .HasOne(cm => cm.Community)
                .WithMany()
                .HasForeignKey(cm => cm.CommunityID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommunityMember>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reaction Relationships
            builder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasOne(r => r.Comment)
                .WithMany()
                .HasForeignKey(r => r.CommentID)
                .OnDelete(DeleteBehavior.Cascade);

            // PostTag Relationships
            builder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany()
                .HasForeignKey(pt => pt.TagID)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatRoom Relationships
            builder.Entity<ChatRoom>()
                .HasOne(cr => cr.Creator)
                .WithMany()
                .HasForeignKey(cr => cr.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ChatRoom>()
                .HasMany(cr => cr.Members)
                .WithOne(crm => crm.Room)
                .HasForeignKey(crm => crm.RoomID)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatRoomMember Relationships
            builder.Entity<ChatRoomMember>()
                .HasOne(crm => crm.User)
                .WithMany()
                .HasForeignKey(crm => crm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatRoomMember>()
                .HasOne(crm => crm.User)
                .WithMany()
                .HasForeignKey(crm => crm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification Relationship
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // Configure Indexes
            // =============================================

            // Posts Indexes
            builder.Entity<Post>()
                .HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Posts_UserId");

            builder.Entity<Post>()
                .HasIndex(p => p.CommunityID)
                .HasDatabaseName("IX_Posts_CommunityID");

            builder.Entity<Post>()
                .HasIndex(p => p.PostStatus)
                .HasDatabaseName("IX_Posts_PostStatus");

            builder.Entity<Post>()
                .HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Posts_CreatedAt");

            builder.Entity<Post>()
                .HasIndex(p => p.Slug)
                .HasDatabaseName("IX_Posts_Slug");

            // Comments Indexes
            builder.Entity<Comment>()
                .HasIndex(c => c.PostID)
                .HasDatabaseName("IX_Comments_PostID");

            builder.Entity<Comment>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Comments_UserId");

            builder.Entity<Comment>()
                .HasIndex(c => c.ParentCommentID)
                .HasDatabaseName("IX_Comments_ParentCommentID");

            // Communities Indexes
            builder.Entity<Community>()
                .HasIndex(c => c.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Communities_Slug");

            builder.Entity<Community>()
                .HasIndex(c => c.CreatorId)
                .HasDatabaseName("IX_Communities_CreatorId");

            builder.Entity<Community>()
                .HasIndex(c => c.CategoryID)
                .HasDatabaseName("IX_Communities_CategoryID");

            // Categories Indexes
            builder.Entity<Category>()
                .HasIndex(c => c.ParentCategoryID)
                .HasDatabaseName("IX_Categories_ParentCategoryID");

            builder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Slug");

            // Tags Indexes
            builder.Entity<Tags>()
                .HasIndex(t => t.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Tags_Slug");

            // UserProfiles Indexes
            builder.Entity<UserProfile>()
                .HasIndex(up => up.UserId)
                .IsUnique()
                .HasDatabaseName("IX_UserProfiles_UserId");

            // CommunityMembers Indexes
            builder.Entity<CommunityMember>()
                .HasIndex(cm => cm.CommunityID)
                .HasDatabaseName("IX_CommunityMembers_CommunityID");

            builder.Entity<CommunityMember>()
                .HasIndex(cm => cm.UserId)
                .HasDatabaseName("IX_CommunityMembers_UserId");

            // Reactions Indexes
            builder.Entity<Reaction>()
                .HasIndex(r => r.PostID)
                .HasDatabaseName("IX_Reactions_PostID");

            builder.Entity<Reaction>()
                .HasIndex(r => r.CommentID)
                .HasDatabaseName("IX_Reactions_CommentID");

            builder.Entity<Reaction>()
                .HasIndex(r => r.UserId)
                .HasDatabaseName("IX_Reactions_UserId");

            // Notifications Indexes
            builder.Entity<Notification>()
                .HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notifications_UserId");

            builder.Entity<Notification>()
                .HasIndex(n => n.IsRead)
                .HasDatabaseName("IX_Notifications_IsRead");

            // MobileAds Indexes
            builder.Entity<MobileAd>()
                .HasIndex(ma => ma.UserId)
                .HasDatabaseName("IX_MobileAds_UserId");

            builder.Entity<MobileAd>()
                .HasIndex(ma => ma.Publish)
                .HasDatabaseName("IX_MobileAds_Publish");

            // =============================================
            // Configure Forum Relationships
            // =============================================

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

            // =============================================
            // Configure Marketplace Relationships
            // =============================================

            builder.Entity<MobileAd>()
                .HasMany(a => a.Images)
                .WithOne(i => i.Ad)
                .HasForeignKey(i => i.AdsId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============================================
            // Configure New Modern Entity Relationships
            // =============================================

            // PostVotes
            builder.Entity<PostVote>()
                .ToTable("PostVotes")
                .HasKey(pv => pv.VoteID);
            builder.Entity<PostVote>()
                .HasOne(pv => pv.Post)
                .WithMany(p => p.Votes)
                .HasForeignKey(pv => pv.PostID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PostVote>()
                .HasOne(pv => pv.User)
                .WithMany()
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PostVote>()
                .HasIndex(pv => new { pv.PostID, pv.UserId })
                .IsUnique();

            // CommentVotes
            builder.Entity<CommentVote>()
                .ToTable("CommentVotes")
                .HasKey(cv => cv.VoteID);
            builder.Entity<CommentVote>()
                .HasOne(cv => cv.Comment)
                .WithMany(c => c.Votes)
                .HasForeignKey(cv => cv.CommentID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CommentVote>()
                .HasOne(cv => cv.User)
                .WithMany()
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CommentVote>()
                .HasIndex(cv => new { cv.CommentID, cv.UserId })
                .IsUnique();

            // PostReports
            builder.Entity<PostReport>()
                .ToTable("PostReports")
                .HasKey(pr => pr.ReportID);
            builder.Entity<PostReport>()
                .HasOne(pr => pr.Post)
                .WithMany(p => p.Reports)
                .HasForeignKey(pr => pr.PostID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PostReport>()
                .HasOne(pr => pr.Reporter)
                .WithMany()
                .HasForeignKey(pr => pr.ReporterUserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<PostReport>()
                .HasOne(pr => pr.Reviewer)
                .WithMany()
                .HasForeignKey(pr => pr.ReviewedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // CommentReports
            builder.Entity<CommentReport>()
                .ToTable("CommentReports")
                .HasKey(cr => cr.ReportID);
            builder.Entity<CommentReport>()
                .HasOne(cr => cr.Comment)
                .WithMany(c => c.Reports)
                .HasForeignKey(cr => cr.CommentID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CommentReport>()
                .HasOne(cr => cr.Reporter)
                .WithMany()
                .HasForeignKey(cr => cr.ReporterUserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<CommentReport>()
                .HasOne(cr => cr.Reviewer)
                .WithMany()
                .HasForeignKey(cr => cr.ReviewedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // UserBlocks
            builder.Entity<UserBlock>()
                .ToTable("UserBlocks")
                .HasKey(ub => ub.BlockID);
            builder.Entity<UserBlock>()
                .HasOne(ub => ub.Blocker)
                .WithMany()
                .HasForeignKey(ub => ub.BlockerUserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserBlock>()
                .HasOne(ub => ub.Blocked)
                .WithMany()
                .HasForeignKey(ub => ub.BlockedUserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<UserBlock>()
                .HasIndex(ub => new { ub.BlockerUserId, ub.BlockedUserId })
                .IsUnique();

            // SocialShares
            builder.Entity<SocialShare>()
                .ToTable("SocialShares")
                .HasKey(ss => ss.ShareID);
            builder.Entity<SocialShare>()
                .HasOne(ss => ss.User)
                .WithMany()
                .HasForeignKey(ss => ss.SharedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // PostViews
            builder.Entity<PostView>()
                .ToTable("PostViews")
                .HasKey(pv => pv.ViewID);
            builder.Entity<PostView>()
                .HasOne(pv => pv.Post)
                .WithMany()
                .HasForeignKey(pv => pv.PostID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PostView>()
                .HasOne(pv => pv.User)
                .WithMany()
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // SavedPosts
            builder.Entity<SavedPost>()
                .ToTable("SavedPosts")
                .HasKey(sp => sp.SavedPostID);
            builder.Entity<SavedPost>()
                .HasOne(sp => sp.Post)
                .WithMany(p => p.SavedBy)
                .HasForeignKey(sp => sp.PostID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<SavedPost>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<SavedPost>()
                .HasIndex(sp => new { sp.PostID, sp.UserId })
                .IsUnique();

            // PostHistory
            builder.Entity<PostHistory>()
                .ToTable("PostHistory")
                .HasKey(ph => ph.HistoryID);
            builder.Entity<PostHistory>()
                .HasOne(ph => ph.Post)
                .WithMany(p => p.History)
                .HasForeignKey(ph => ph.PostID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PostHistory>()
                .HasOne(ph => ph.Editor)
                .WithMany()
                .HasForeignKey(ph => ph.EditedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // ForumVotes
            builder.Entity<ForumVote>()
                .ToTable("ForumVotes")
                .HasKey(fv => fv.VoteID);
            builder.Entity<ForumVote>()
                .HasOne(fv => fv.ForumThread)
                .WithMany()
                .HasForeignKey(fv => fv.ForumThreadID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ForumVote>()
                .HasOne(fv => fv.User)
                .WithMany()
                .HasForeignKey(fv => fv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ForumVote>()
                .HasIndex(fv => new { fv.ForumThreadID, fv.UserId })
                .IsUnique();
        }
    }
}

