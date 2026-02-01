-- SQL Migration Script: Add Missing Columns for Full PRD Compliance
-- Database: gsmsharingv3
-- Date: 2025-02-01

-- ============================================
-- 1. ADD COLUMNS TO Posts TABLE
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'CanonicalUrl'
)
BEGIN
    ALTER TABLE dbo.Posts ADD CanonicalUrl NVARCHAR(MAX) NULL;
    PRINT 'Added CanonicalUrl column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'FocusKeyword'
)
BEGIN
    ALTER TABLE dbo.Posts ADD FocusKeyword NVARCHAR(MAX) NULL;
    PRINT 'Added FocusKeyword column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Excerpt'
)
BEGIN
    ALTER TABLE dbo.Posts ADD Excerpt NVARCHAR(MAX) NULL;
    PRINT 'Added Excerpt column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Score'
)
BEGIN
    ALTER TABLE dbo.Posts ADD Score INT NOT NULL DEFAULT 0;
    PRINT 'Added Score column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'CommentCount'
)
BEGIN
    ALTER TABLE dbo.Posts ADD CommentCount INT NOT NULL DEFAULT 0;
    PRINT 'Added CommentCount column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'UpvoteCount'
)
BEGIN
    ALTER TABLE dbo.Posts ADD UpvoteCount INT NOT NULL DEFAULT 0;
    PRINT 'Added UpvoteCount column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DownvoteCount'
)
BEGIN
    ALTER TABLE dbo.Posts ADD DownvoteCount INT NOT NULL DEFAULT 0;
    PRINT 'Added DownvoteCount column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsLocked'
)
BEGIN
    ALTER TABLE dbo.Posts ADD IsLocked BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsLocked column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsPinned'
)
BEGIN
    ALTER TABLE dbo.Posts ADD IsPinned BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsPinned column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE dbo.Posts ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DeletedAt'
)
BEGIN
    ALTER TABLE dbo.Posts ADD DeletedAt DATETIME2 NULL;
    PRINT 'Added DeletedAt column to Posts table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SchemaMarkup'
)
BEGIN
    ALTER TABLE dbo.Posts ADD SchemaMarkup NVARCHAR(MAX) NULL;
    PRINT 'Added SchemaMarkup column to Posts table';
END

-- ============================================
-- 2. ADD COLUMNS TO Comments TABLE
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'UpvoteCount'
)
BEGIN
    ALTER TABLE dbo.Comments ADD UpvoteCount INT NOT NULL DEFAULT 0;
    PRINT 'Added UpvoteCount column to Comments table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'DownvoteCount'
)
BEGIN
    ALTER TABLE dbo.Comments ADD DownvoteCount INT NOT NULL DEFAULT 0;
    PRINT 'Added DownvoteCount column to Comments table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'IsEdited'
)
BEGIN
    ALTER TABLE dbo.Comments ADD IsEdited BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsEdited column to Comments table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'EditedAt'
)
BEGIN
    ALTER TABLE dbo.Comments ADD EditedAt DATETIME2 NULL;
    PRINT 'Added EditedAt column to Comments table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE dbo.Comments ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Comments table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'DeletedAt'
)
BEGIN
    ALTER TABLE dbo.Comments ADD DeletedAt DATETIME2 NULL;
    PRINT 'Added DeletedAt column to Comments table';
END

-- ============================================
-- 3. ADD COLUMNS TO Communities TABLE
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'MetaTitle'
)
BEGIN
    ALTER TABLE dbo.Communities ADD MetaTitle NVARCHAR(MAX) NULL;
    PRINT 'Added MetaTitle column to Communities table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'MetaDescription'
)
BEGIN
    ALTER TABLE dbo.Communities ADD MetaDescription NVARCHAR(MAX) NULL;
    PRINT 'Added MetaDescription column to Communities table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE dbo.Communities ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Communities table';
END

-- ============================================
-- 4. ADD COLUMNS TO AspNetUsers TABLE (ApplicationUser)
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'AvatarPath'
)
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD AvatarPath NVARCHAR(MAX) NULL;
    PRINT 'Added AvatarPath column to AspNetUsers table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'City'
)
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD City NVARCHAR(MAX) NULL;
    PRINT 'Added City column to AspNetUsers table';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'CreatedDate'
)
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD CreatedDate DATETIME2 NULL;
    PRINT 'Added CreatedDate column to AspNetUsers table';
END

-- ============================================
-- 5. ADD INDEXES FOR PERFORMANCE
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Posts_Slug' AND object_id = OBJECT_ID('dbo.Posts')
)
BEGIN
    CREATE UNIQUE INDEX IX_Posts_Slug ON dbo.Posts(Slug);
    PRINT 'Created IX_Posts_Slug index';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Communities_Slug' AND object_id = OBJECT_ID('dbo.Communities')
)
BEGIN
    CREATE UNIQUE INDEX IX_Communities_Slug ON dbo.Communities(Slug);
    PRINT 'Created IX_Communities_Slug index';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Posts_PostStatus' AND object_id = OBJECT_ID('dbo.Posts')
)
BEGIN
    CREATE INDEX IX_Posts_PostStatus ON dbo.Posts(PostStatus);
    PRINT 'Created IX_Posts_PostStatus index';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Comments_PostID' AND object_id = OBJECT_ID('dbo.Comments')
)
BEGIN
    CREATE INDEX IX_Comments_PostID ON dbo.Comments(PostID);
    PRINT 'Created IX_Comments_PostID index';
END

-- ============================================
-- 6. ADD NEW TABLES FOR FORUM SYSTEM
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ForumThreads')
BEGIN
    CREATE TABLE [dbo].[ForumThreads] (
        [ThreadID] INT IDENTITY(1,1) PRIMARY KEY,
        [CategoryID] INT NULL,
        [UserID] NVARCHAR(450) NULL,
        [Title] NVARCHAR(500) NULL,
        [Slug] NVARCHAR(500) NULL,
        [Content] NVARCHAR(MAX) NULL,
        [Views] INT DEFAULT 0,
        [Likes] INT DEFAULT 0,
        [Dislikes] INT DEFAULT 0,
        [IsPinned] BIT DEFAULT 0,
        [IsLocked] BIT DEFAULT 0,
        [Publish] BIT DEFAULT 0,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    PRINT 'Created ForumThreads table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ForumReplies')
BEGIN
    CREATE TABLE [dbo].[ForumReplies] (
        [ReplyID] INT IDENTITY(1,1) PRIMARY KEY,
        [ThreadID] INT NULL,
        [UserID] NVARCHAR(450) NULL,
        [Content] NVARCHAR(MAX) NULL,
        [IsAnswer] BIT DEFAULT 0,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    PRINT 'Created ForumReplies table';
END

-- ============================================
-- 7. ADD NEW TABLES FOR MOBILE SPECS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MobileBrands')
BEGIN
    CREATE TABLE [dbo].[MobileBrands] (
        [BrandID] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NULL,
        [Slug] NVARCHAR(100) NULL,
        [LogoUrl] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 DEFAULT GETDATE()
    );
    PRINT 'Created MobileBrands table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MobileModels')
BEGIN
    CREATE TABLE [dbo].[MobileModels] (
        [ModelID] INT IDENTITY(1,1) PRIMARY KEY,
        [BrandID] INT NULL,
        [Name] NVARCHAR(200) NULL,
        [Slug] NVARCHAR(200) NULL,
        [Network] NVARCHAR(MAX) NULL,
        [LaunchDate] DATETIME2 NULL,
        [Body] NVARCHAR(MAX) NULL,
        [Display] NVARCHAR(MAX) NULL,
        [Platform] NVARCHAR(MAX) NULL,
        [Memory] NVARCHAR(MAX) NULL,
        [MainCamera] NVARCHAR(MAX) NULL,
        [SelfieCamera] NVARCHAR(MAX) NULL,
        [Sound] NVARCHAR(MAX) NULL,
        [Comms] NVARCHAR(MAX) NULL,
        [Features] NVARCHAR(MAX) NULL,
        [Battery] NVARCHAR(MAX) NULL,
        [Price] DECIMAL(10,2) NULL,
        [ImageUrl] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    PRINT 'Created MobileModels table';
END

-- ============================================
-- 8. ADD NEW TABLES FOR MOBILE PARTS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MobilePartCategories')
BEGIN
    CREATE TABLE [dbo].[MobilePartCategories] (
        [CategoryID] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NULL,
        [Slug] NVARCHAR(100) NULL,
        [Description] NVARCHAR(MAX) NULL,
        [IconClass] NVARCHAR(100) NULL,
        [ParentID] INT NULL,
        [CreatedAt] DATETIME2 DEFAULT GETDATE()
    );
    PRINT 'Created MobilePartCategories table';
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MobilePartCompatibility')
BEGIN
    CREATE TABLE [dbo].[MobilePartCompatibility] (
        [CompatibilityID] INT IDENTITY(1,1) PRIMARY KEY,
        [PartID] INT NULL,
        [BrandID] INT NULL,
        [ModelID] INT NULL,
        [CreatedAt] DATETIME2 DEFAULT GETDATE()
    );
    PRINT 'Created MobilePartCompatibility table';
END

-- ============================================
-- 9. ADD NEW TABLES FOR SEARCH
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SearchHistory')
BEGIN
    CREATE TABLE [dbo].[SearchHistory] (
        [SearchID] INT IDENTITY(1,1) PRIMARY KEY,
        [UserID] NVARCHAR(450) NULL,
        [Query] NVARCHAR(500) NULL,
        [ResultCount] INT DEFAULT 0,
        [CreatedAt] DATETIME2 DEFAULT GETDATE()
    );
    PRINT 'Created SearchHistory table';
END

-- ============================================
-- 10. ADD NEW TABLES FOR NOTIFICATIONS
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences')
BEGIN
    CREATE TABLE [dbo].[NotificationPreferences] (
        [PreferenceID] INT IDENTITY(1,1) PRIMARY KEY,
        [UserID] NVARCHAR(450) NULL,
        [EmailOnComment] BIT DEFAULT 1,
        [EmailOnReply] BIT DEFAULT 1,
        [EmailOnMention] BIT DEFAULT 1,
        [EmailOnFollow] BIT DEFAULT 1,
        [PushOnComment] BIT DEFAULT 1,
        [PushOnReply] BIT DEFAULT 1,
        [PushOnMention] BIT DEFAULT 1,
        [PushOnFollow] BIT DEFAULT 1,
        [CreatedAt] DATETIME2 DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    PRINT 'Created NotificationPreferences table';
END

PRINT '============================================';
PRINT 'Migration completed successfully!';
PRINT '============================================';
GO
