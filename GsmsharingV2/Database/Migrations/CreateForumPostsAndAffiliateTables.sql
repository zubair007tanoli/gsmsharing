-- ============================================================
-- GSMSharing V2 - Forum Posts & Enhanced Affiliate Marketing
-- Database Migration Script for gsmsharingv4 (New Database)
-- ============================================================
-- This script creates tables for:
-- 1. Reddit-style Forum Posts with photos and links
-- 2. Enhanced Affiliate Marketing system
-- ============================================================

USE [gsmsharingv4];
GO

-- ============================================================
-- 1. FORUM POSTS SYSTEM (Reddit-Style)
-- ============================================================

-- Forum Posts Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ForumPosts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ForumPosts] (
        [ForumPostID] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] BIGINT NULL,
        [CommunityID] BIGINT NULL,
        
        -- Post Type
        [PostType] NVARCHAR(50) NOT NULL DEFAULT 'text', -- 'text', 'image', 'link', 'video'
        
        -- Content
        [Title] NVARCHAR(500) NOT NULL,
        [Content] NVARCHAR(MAX) NULL, -- For text posts
        [Slug] NVARCHAR(500) NULL,
        
        -- Link Post Fields
        [LinkUrl] NVARCHAR(1000) NULL, -- For link posts
        [LinkTitle] NVARCHAR(500) NULL,
        [LinkDescription] NVARCHAR(1000) NULL,
        [LinkThumbnail] NVARCHAR(1000) NULL,
        
        -- Engagement Metrics
        [ViewCount] INT DEFAULT 0,
        [Score] INT DEFAULT 0,
        [UpvoteCount] INT DEFAULT 0,
        [DownvoteCount] INT DEFAULT 0,
        [CommentCount] INT DEFAULT 0,
        
        -- Status Flags
        [IsPinned] BIT DEFAULT 0,
        [IsLocked] BIT DEFAULT 0,
        [IsDeleted] BIT DEFAULT 0,
        [PostStatus] NVARCHAR(50) DEFAULT 'published', -- 'draft', 'published', 'archived'
        
        -- Timestamps
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [PublishedAt] DATETIME2 NULL,
        [DeletedAt] DATETIME2 NULL,
        
        CONSTRAINT [PK_ForumPosts] PRIMARY KEY CLUSTERED ([ForumPostID] ASC)
    );
    
    -- Indexes
    CREATE INDEX [IX_ForumPosts_CommunityID] ON [dbo].[ForumPosts]([CommunityID]);
    CREATE INDEX [IX_ForumPosts_UserId] ON [dbo].[ForumPosts]([UserId]);
    CREATE INDEX [IX_ForumPosts_PostType] ON [dbo].[ForumPosts]([PostType]);
    CREATE INDEX [IX_ForumPosts_CreatedAt] ON [dbo].[ForumPosts]([CreatedAt] DESC);
    CREATE INDEX [IX_ForumPosts_Score] ON [dbo].[ForumPosts]([Score] DESC);
    CREATE INDEX [IX_ForumPosts_PostStatus] ON [dbo].[ForumPosts]([PostStatus]);
    CREATE INDEX [IX_ForumPosts_Slug] ON [dbo].[ForumPosts]([Slug]);
    
    PRINT 'Table [ForumPosts] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [ForumPosts] already exists.';
END
GO

-- Post Media Attachments Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostMedia]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostMedia] (
        [MediaID] BIGINT IDENTITY(1,1) NOT NULL,
        [PostID] BIGINT NULL, -- Can link to Posts table
        [ForumPostID] BIGINT NULL, -- Specific to forum posts
        [MediaType] NVARCHAR(50) NOT NULL, -- 'image', 'video', 'gif'
        [MediaUrl] NVARCHAR(1000) NOT NULL,
        [ThumbnailUrl] NVARCHAR(1000) NULL,
        [AltText] NVARCHAR(500) NULL,
        [DisplayOrder] INT DEFAULT 0,
        [FileSize] BIGINT NULL,
        [Width] INT NULL,
        [Height] INT NULL,
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_PostMedia] PRIMARY KEY CLUSTERED ([MediaID] ASC),
        CONSTRAINT [FK_PostMedia_ForumPosts] FOREIGN KEY ([ForumPostID]) 
            REFERENCES [dbo].[ForumPosts]([ForumPostID]) ON DELETE CASCADE
    );
    
    -- Indexes
    CREATE INDEX [IX_PostMedia_PostID] ON [dbo].[PostMedia]([PostID]);
    CREATE INDEX [IX_PostMedia_ForumPostID] ON [dbo].[PostMedia]([ForumPostID]);
    CREATE INDEX [IX_PostMedia_MediaType] ON [dbo].[PostMedia]([MediaType]);
    
    PRINT 'Table [PostMedia] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [PostMedia] already exists.';
END
GO

-- Forum Post Votes Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ForumPostVotes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ForumPostVotes] (
        [VoteID] BIGINT IDENTITY(1,1) NOT NULL,
        [ForumPostID] BIGINT NOT NULL,
        [UserId] BIGINT NOT NULL,
        [VoteType] NVARCHAR(10) NOT NULL, -- 'upvote', 'downvote'
        [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_ForumPostVotes] PRIMARY KEY CLUSTERED ([VoteID] ASC),
        CONSTRAINT [FK_ForumPostVotes_ForumPosts] FOREIGN KEY ([ForumPostID]) 
            REFERENCES [dbo].[ForumPosts]([ForumPostID]) ON DELETE CASCADE,
        CONSTRAINT [UQ_ForumPostVotes_UserPost] UNIQUE ([ForumPostID], [UserId])
    );
    
    -- Indexes
    CREATE INDEX [IX_ForumPostVotes_ForumPostID] ON [dbo].[ForumPostVotes]([ForumPostID]);
    CREATE INDEX [IX_ForumPostVotes_UserId] ON [dbo].[ForumPostVotes]([UserId]);
    CREATE INDEX [IX_ForumPostVotes_VoteType] ON [dbo].[ForumPostVotes]([VoteType]);
    
    PRINT 'Table [ForumPostVotes] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [ForumPostVotes] already exists.';
END
GO

-- ============================================================
-- 2. ENHANCED AFFILIATE MARKETING SYSTEM
-- ============================================================

-- Update AffiliatePartners Table (if exists, add new columns)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND type in (N'U'))
BEGIN
    -- Add PartnerType if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND name = 'PartnerType')
    BEGIN
        ALTER TABLE [dbo].[AffiliatePartners]
        ADD [PartnerType] NVARCHAR(50) NOT NULL DEFAULT 'amazon'; -- 'amazon', 'aliexpress', 'other'
        PRINT 'Column [PartnerType] added to [AffiliatePartners].';
    END
    
    -- Add TrackingId if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND name = 'TrackingId')
    BEGIN
        ALTER TABLE [dbo].[AffiliatePartners]
        ADD [TrackingId] NVARCHAR(100) NULL; -- Amazon Associate Tag / AliExpress PID
        PRINT 'Column [TrackingId] added to [AffiliatePartners].';
    END
    
    -- Add ApiKey if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND name = 'ApiKey')
    BEGIN
        ALTER TABLE [dbo].[AffiliatePartners]
        ADD [ApiKey] NVARCHAR(500) NULL;
        PRINT 'Column [ApiKey] added to [AffiliatePartners].';
    END
    
    -- Add ApiSecret if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND name = 'ApiSecret')
    BEGIN
        ALTER TABLE [dbo].[AffiliatePartners]
        ADD [ApiSecret] NVARCHAR(500) NULL;
        PRINT 'Column [ApiSecret] added to [AffiliatePartners].';
    END
    
    -- Add CommissionRate if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND name = 'CommissionRate')
    BEGIN
        ALTER TABLE [dbo].[AffiliatePartners]
        ADD [CommissionRate] DECIMAL(5,2) NULL;
        PRINT 'Column [CommissionRate] added to [AffiliatePartners].';
    END
    
    -- Add IsActive if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliatePartners]') AND name = 'IsActive')
    BEGIN
        ALTER TABLE [dbo].[AffiliatePartners]
        ADD [IsActive] BIT DEFAULT 1;
        PRINT 'Column [IsActive] added to [AffiliatePartners].';
    END
END
ELSE
BEGIN
    -- Create AffiliatePartners table if it doesn't exist
    CREATE TABLE [dbo].[AffiliatePartners] (
        [PartnerID] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(255) NOT NULL,
        [PartnerType] NVARCHAR(50) NOT NULL DEFAULT 'amazon',
        [AffiliateTag] NVARCHAR(255) NULL,
        [BaseUrl] NVARCHAR(500) NULL,
        [TrackingId] NVARCHAR(100) NULL,
        [ApiKey] NVARCHAR(500) NULL,
        [ApiSecret] NVARCHAR(500) NULL,
        [CommissionRate] DECIMAL(5,2) NULL,
        [IsActive] BIT DEFAULT 1,
        
        CONSTRAINT [PK_AffiliatePartners] PRIMARY KEY CLUSTERED ([PartnerID] ASC)
    );
    
    CREATE INDEX [IX_AffiliatePartners_PartnerType] ON [dbo].[AffiliatePartners]([PartnerType]);
    CREATE INDEX [IX_AffiliatePartners_IsActive] ON [dbo].[AffiliatePartners]([IsActive]);
    
    PRINT 'Table [AffiliatePartners] created successfully.';
END
GO

-- Update AffiliateProducts Table (if exists, add new columns)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND type in (N'U'))
BEGIN
    -- Add ASIN if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'ASIN')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [ASIN] NVARCHAR(20) NULL;
        PRINT 'Column [ASIN] added to [AffiliateProducts].';
    END
    
    -- Add AliExpressProductId if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'AliExpressProductId')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [AliExpressProductId] NVARCHAR(50) NULL;
        PRINT 'Column [AliExpressProductId] added to [AffiliateProducts].';
    END
    
    -- Add OriginalPrice if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'OriginalPrice')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [OriginalPrice] DECIMAL(10,2) NULL;
        PRINT 'Column [OriginalPrice] added to [AffiliateProducts].';
    END
    
    -- Add DiscountPrice if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'DiscountPrice')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [DiscountPrice] DECIMAL(10,2) NULL;
        PRINT 'Column [DiscountPrice] added to [AffiliateProducts].';
    END
    
    -- Add Currency if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'Currency')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [Currency] NVARCHAR(10) DEFAULT 'USD';
        PRINT 'Column [Currency] added to [AffiliateProducts].';
    END
    
    -- Add Rating if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'Rating')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [Rating] DECIMAL(3,2) NULL;
        PRINT 'Column [Rating] added to [AffiliateProducts].';
    END
    
    -- Add ReviewCount if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'ReviewCount')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [ReviewCount] INT NULL;
        PRINT 'Column [ReviewCount] added to [AffiliateProducts].';
    END
    
    -- Add Availability if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'Availability')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [Availability] NVARCHAR(50) NULL;
        PRINT 'Column [Availability] added to [AffiliateProducts].';
    END
    
    -- Add PrimeEligible if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'PrimeEligible')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [PrimeEligible] BIT NULL;
        PRINT 'Column [PrimeEligible] added to [AffiliateProducts].';
    END
    
    -- Add BestSeller if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'BestSeller')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [BestSeller] BIT NULL;
        PRINT 'Column [BestSeller] added to [AffiliateProducts].';
    END
    
    -- Add AmazonChoice if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'AmazonChoice')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [AmazonChoice] BIT NULL;
        PRINT 'Column [AmazonChoice] added to [AffiliateProducts].';
    END
    
    -- Add ProductCategory if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'ProductCategory')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [ProductCategory] NVARCHAR(100) NULL;
        PRINT 'Column [ProductCategory] added to [AffiliateProducts].';
    END
    
    -- Add Brand if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]') AND name = 'Brand')
    BEGIN
        ALTER TABLE [dbo].[AffiliateProducts]
        ADD [Brand] NVARCHAR(100) NULL;
        PRINT 'Column [Brand] added to [AffiliateProducts].';
    END
    
    -- Add indexes
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AffiliateProducts_ASIN' AND object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]'))
    BEGIN
        CREATE INDEX [IX_AffiliateProducts_ASIN] ON [dbo].[AffiliateProducts]([ASIN]);
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AffiliateProducts_Rating' AND object_id = OBJECT_ID(N'[dbo].[AffiliateProducts]'))
    BEGIN
        CREATE INDEX [IX_AffiliateProducts_Rating] ON [dbo].[AffiliateProducts]([Rating] DESC);
    END
END
GO

-- Affiliate Link Clicks Table (Enhanced)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateClicks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AffiliateClicks] (
        [ClickID] BIGINT IDENTITY(1,1) NOT NULL,
        [ProductID] BIGINT NULL,
        [UserId] BIGINT NULL,
        [IPAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [ReferrerUrl] NVARCHAR(1000) NULL,
        [ClickDate] DATETIME2 DEFAULT GETUTCDATE(),
        [Converted] BIT DEFAULT 0, -- If purchase was made
        [ConversionDate] DATETIME2 NULL,
        [CommissionAmount] DECIMAL(10,2) NULL,
        
        CONSTRAINT [PK_AffiliateClicks] PRIMARY KEY CLUSTERED ([ClickID] ASC)
    );
    
    -- Indexes
    CREATE INDEX [IX_AffiliateClicks_ProductID] ON [dbo].[AffiliateClicks]([ProductID]);
    CREATE INDEX [IX_AffiliateClicks_UserId] ON [dbo].[AffiliateClicks]([UserId]);
    CREATE INDEX [IX_AffiliateClicks_ClickDate] ON [dbo].[AffiliateClicks]([ClickDate] DESC);
    CREATE INDEX [IX_AffiliateClicks_Converted] ON [dbo].[AffiliateClicks]([Converted]);
    
    PRINT 'Table [AffiliateClicks] created successfully.';
END
ELSE
BEGIN
    -- Add new columns if table exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateClicks]') AND name = 'IPAddress')
    BEGIN
        ALTER TABLE [dbo].[AffiliateClicks]
        ADD [IPAddress] NVARCHAR(50) NULL,
            [UserAgent] NVARCHAR(500) NULL,
            [ReferrerUrl] NVARCHAR(1000) NULL,
            [Converted] BIT DEFAULT 0,
            [ConversionDate] DATETIME2 NULL,
            [CommissionAmount] DECIMAL(10,2) NULL;
        PRINT 'Enhanced columns added to [AffiliateClicks].';
    END
END
GO

-- ============================================================
-- 3. SEED DATA (Optional)
-- ============================================================

-- Seed Affiliate Partners (Amazon and AliExpress)
IF NOT EXISTS (SELECT * FROM [dbo].[AffiliatePartners] WHERE [PartnerType] = 'amazon')
BEGIN
    INSERT INTO [dbo].[AffiliatePartners] ([Name], [PartnerType], [BaseUrl], [IsActive])
    VALUES ('Amazon Associates', 'amazon', 'https://www.amazon.com', 1);
    PRINT 'Amazon affiliate partner seeded.';
END

IF NOT EXISTS (SELECT * FROM [dbo].[AffiliatePartners] WHERE [PartnerType] = 'aliexpress')
BEGIN
    INSERT INTO [dbo].[AffiliatePartners] ([Name], [PartnerType], [BaseUrl], [IsActive])
    VALUES ('AliExpress Affiliate', 'aliexpress', 'https://www.aliexpress.com', 1);
    PRINT 'AliExpress affiliate partner seeded.';
END
GO

PRINT '============================================================';
PRINT 'Migration completed successfully!';
PRINT '============================================================';
PRINT 'New tables created:';
PRINT '  - ForumPosts (Reddit-style forum posts)';
PRINT '  - PostMedia (Media attachments)';
PRINT '  - ForumPostVotes (Voting system)';
PRINT '';
PRINT 'Enhanced tables:';
PRINT '  - AffiliatePartners (with PartnerType, TrackingId, etc.)';
PRINT '  - AffiliateProducts (with ASIN, Rating, etc.)';
PRINT '  - AffiliateClicks (with enhanced tracking)';
PRINT '============================================================';
GO








