-- ================================================================
-- MODERNIZED GSMSharing Database Schema
-- Updated: 2025-01-XX
-- Standards: Modern SQL Server, SEO Optimized, Production Ready
-- ================================================================

USE [gsmsharingv3]
GO

-- ================================================================
-- PART 1: DROP OLD TESTING SCHEMA AND TABLES (gsmsharing schema)
-- ================================================================
PRINT 'Starting database modernization...'

-- Drop all tables in gsmsharing schema
IF SCHEMA_ID('gsmsharing') IS NOT NULL
BEGIN
    -- Drop foreign keys first, then tables
    DECLARE @sql NVARCHAR(MAX) = N''
    SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
                   ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
    FROM sys.foreign_keys
    WHERE OBJECT_SCHEMA_NAME(referenced_object_id) = 'gsmsharing'
    
    EXEC sp_executesql @sql
    
    -- Drop tables in gmsharing schema
    DECLARE @dropSql NVARCHAR(MAX) = N''
    SELECT @dropSql += N'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
    FROM sys.tables
    WHERE SCHEMA_NAME(schema_id) = 'gsmsharing'
    
    EXEC sp_executesql @dropSql
    
    -- Drop schema
    DROP SCHEMA [gsmsharing]
    PRINT 'Dropped gsmsharing schema and all associated tables'
END
GO

-- ================================================================
-- PART 2: MODERNIZE EXISTING TABLES - ADD MISSING COLUMNS & INDEXES
-- ================================================================

-- Add SEO and modern fields to Posts table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Posts')
BEGIN
    -- Add missing modern columns
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'CanonicalUrl')
        ALTER TABLE [dbo].[Posts] ADD [CanonicalUrl] [nvarchar](500) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'FocusKeyword')
        ALTER TABLE [dbo].[Posts] ADD [FocusKeyword] [nvarchar](255) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Excerpt')
        ALTER TABLE [dbo].[Posts] ADD [Excerpt] [nvarchar](500) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Score')
        ALTER TABLE [dbo].[Posts] ADD [Score] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'CommentCount')
        ALTER TABLE [dbo].[Posts] ADD [CommentCount] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'UpvoteCount')
        ALTER TABLE [dbo].[Posts] ADD [UpvoteCount] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DownvoteCount')
        ALTER TABLE [dbo].[Posts] ADD [DownvoteCount] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsLocked')
        ALTER TABLE [dbo].[Posts] ADD [IsLocked] [bit] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsPinned')
        ALTER TABLE [dbo].[Posts] ADD [IsPinned] [bit] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsDeleted')
        ALTER TABLE [dbo].[Posts] ADD [IsDeleted] [bit] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DeletedAt')
        ALTER TABLE [dbo].[Posts] ADD [DeletedAt] [datetime2](7) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SchemaMarkup')
        ALTER TABLE [dbo].[Posts] ADD [SchemaMarkup] [nvarchar](max) NULL -- JSON-LD structured data
    
    -- Add indexes for performance
    -- Note: Slug index skipped if Slug is nvarchar(max) - cannot index max columns
    -- Consider altering Slug to nvarchar(450) or nvarchar(500) if you need this index
    
    -- Add Score index (only if Score column exists)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Score')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_Score' AND object_id = OBJECT_ID('dbo.Posts'))
            CREATE NONCLUSTERED INDEX [IX_Posts_Score] ON [dbo].[Posts]([Score] DESC, [CreatedAt] DESC)
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_Status_Community' AND object_id = OBJECT_ID('dbo.Posts'))
        CREATE NONCLUSTERED INDEX [IX_Posts_Status_Community] ON [dbo].[Posts]([PostStatus], [CommunityID], [CreatedAt] DESC)
    
    PRINT 'Posts table modernized'
END
GO

-- Modernize Comments table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'UpvoteCount')
        ALTER TABLE [dbo].[Comments] ADD [UpvoteCount] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'DownvoteCount')
        ALTER TABLE [dbo].[Comments] ADD [DownvoteCount] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'IsEdited')
        ALTER TABLE [dbo].[Comments] ADD [IsEdited] [bit] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'EditedAt')
        ALTER TABLE [dbo].[Comments] ADD [EditedAt] [datetime2](7) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'IsDeleted')
        ALTER TABLE [dbo].[Comments] ADD [IsDeleted] [bit] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'DeletedAt')
        ALTER TABLE [dbo].[Comments] ADD [DeletedAt] [datetime2](7) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_PostID_Parent' AND object_id = OBJECT_ID('dbo.Comments'))
        CREATE NONCLUSTERED INDEX [IX_Comments_PostID_Parent] ON [dbo].[Comments]([PostID], [ParentCommentID], [CreatedAt] DESC)
    
    PRINT 'Comments table modernized'
END
GO

-- Modernize Communities table with SEO
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Communities')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'MetaTitle')
        ALTER TABLE [dbo].[Communities] ADD [MetaTitle] [nvarchar](255) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'MetaDescription')
        ALTER TABLE [dbo].[Communities] ADD [MetaDescription] [nvarchar](500) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'IsDeleted')
        ALTER TABLE [dbo].[Communities] ADD [IsDeleted] [bit] NOT NULL DEFAULT(0)
    
    -- Add unique index on Slug (only if Slug is not nvarchar(max))
    -- Note: If Slug is nvarchar(max), you'll need to alter it to a fixed length first
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'Slug' AND max_length != -1)
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Communities_Slug_Unique' AND object_id = OBJECT_ID('dbo.Communities'))
        BEGIN
            -- Check if IsDeleted column exists for filtered index
            IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Communities') AND name = 'IsDeleted')
                CREATE UNIQUE NONCLUSTERED INDEX [IX_Communities_Slug_Unique] ON [dbo].[Communities]([Slug]) WHERE [IsDeleted] = 0 AND [Slug] IS NOT NULL
            ELSE
                CREATE UNIQUE NONCLUSTERED INDEX [IX_Communities_Slug_Unique] ON [dbo].[Communities]([Slug]) WHERE [Slug] IS NOT NULL
        END
    END
    
    PRINT 'Communities table modernized'
END
GO

-- Modernize UserProfiles table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserProfiles')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserProfiles') AND name = 'Reputation')
        ALTER TABLE [dbo].[UserProfiles] ADD [Reputation] [int] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserProfiles') AND name = 'IsBanned')
        ALTER TABLE [dbo].[UserProfiles] ADD [IsBanned] [bit] NOT NULL DEFAULT(0)
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserProfiles') AND name = 'BannedUntil')
        ALTER TABLE [dbo].[UserProfiles] ADD [BannedUntil] [datetime2](7) NULL
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserProfiles') AND name = 'BanReason')
        ALTER TABLE [dbo].[UserProfiles] ADD [BanReason] [nvarchar](max) NULL
    
    PRINT 'UserProfiles table modernized'
END
GO

-- ================================================================
-- PART 3: CREATE NEW MODERN TABLES
-- ================================================================

-- Post Reports Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PostReports')
BEGIN
    CREATE TABLE [dbo].[PostReports](
        [ReportID] [int] IDENTITY(1,1) NOT NULL,
        [PostID] [int] NOT NULL,
        [ReporterUserId] [nvarchar](450) NOT NULL,
        [ReportReason] [nvarchar](50) NOT NULL, -- spam, harassment, inappropriate, copyright, other
        [ReportDetails] [nvarchar](max) NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT('pending'), -- pending, reviewed, dismissed, action_taken
        [ReviewedBy] [nvarchar](450) NULL,
        [ReviewedAt] [datetime2](7) NULL,
        [ReviewNotes] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_PostReports] PRIMARY KEY CLUSTERED ([ReportID] ASC),
        CONSTRAINT [FK_PostReports_Posts] FOREIGN KEY([PostID]) REFERENCES [dbo].[Posts] ([PostID]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostReports_Reporter] FOREIGN KEY([ReporterUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PostReports_Reviewer] FOREIGN KEY([ReviewedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_PostReports_Reason] CHECK ([ReportReason] IN ('spam', 'harassment', 'inappropriate', 'copyright', 'misinformation', 'other'))
    )
    
    CREATE NONCLUSTERED INDEX [IX_PostReports_PostID] ON [dbo].[PostReports]([PostID])
    CREATE NONCLUSTERED INDEX [IX_PostReports_Status] ON [dbo].[PostReports]([Status], [CreatedAt] DESC)
    CREATE NONCLUSTERED INDEX [IX_PostReports_Reporter] ON [dbo].[PostReports]([ReporterUserId])
    
    PRINT 'PostReports table created'
END
GO

-- Comment Reports Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommentReports')
BEGIN
    CREATE TABLE [dbo].[CommentReports](
        [ReportID] [int] IDENTITY(1,1) NOT NULL,
        [CommentID] [int] NOT NULL,
        [ReporterUserId] [nvarchar](450) NOT NULL,
        [ReportReason] [nvarchar](50) NOT NULL,
        [ReportDetails] [nvarchar](max) NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT('pending'),
        [ReviewedBy] [nvarchar](450) NULL,
        [ReviewedAt] [datetime2](7) NULL,
        [ReviewNotes] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_CommentReports] PRIMARY KEY CLUSTERED ([ReportID] ASC),
        CONSTRAINT [FK_CommentReports_Comments] FOREIGN KEY([CommentID]) REFERENCES [dbo].[Comments] ([CommentID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommentReports_Reporter] FOREIGN KEY([ReporterUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CommentReports_Reviewer] FOREIGN KEY([ReviewedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CK_CommentReports_Reason] CHECK ([ReportReason] IN ('spam', 'harassment', 'inappropriate', 'copyright', 'misinformation', 'other'))
    )
    
    CREATE NONCLUSTERED INDEX [IX_CommentReports_CommentID] ON [dbo].[CommentReports]([CommentID])
    CREATE NONCLUSTERED INDEX [IX_CommentReports_Status] ON [dbo].[CommentReports]([Status], [CreatedAt] DESC)
    
    PRINT 'CommentReports table created'
END
GO

-- User Blocks Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserBlocks')
BEGIN
    CREATE TABLE [dbo].[UserBlocks](
        [BlockID] [int] IDENTITY(1,1) NOT NULL,
        [BlockerUserId] [nvarchar](450) NOT NULL, -- User who is blocking
        [BlockedUserId] [nvarchar](450) NOT NULL, -- User who is being blocked
        [Reason] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_UserBlocks] PRIMARY KEY CLUSTERED ([BlockID] ASC),
        CONSTRAINT [FK_UserBlocks_Blocker] FOREIGN KEY([BlockerUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserBlocks_Blocked] FOREIGN KEY([BlockedUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [UQ_UserBlocks_Unique] UNIQUE NONCLUSTERED ([BlockerUserId] ASC, [BlockedUserId] ASC),
        CONSTRAINT [CK_UserBlocks_SelfBlock] CHECK ([BlockerUserId] != [BlockedUserId])
    )
    
    CREATE NONCLUSTERED INDEX [IX_UserBlocks_Blocker] ON [dbo].[UserBlocks]([BlockerUserId])
    CREATE NONCLUSTERED INDEX [IX_UserBlocks_Blocked] ON [dbo].[UserBlocks]([BlockedUserId])
    
    PRINT 'UserBlocks table created'
END
GO

-- Post Votes Table (Modern Voting System)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PostVotes')
BEGIN
    CREATE TABLE [dbo].[PostVotes](
        [VoteID] [int] IDENTITY(1,1) NOT NULL,
        [PostID] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [VoteType] [int] NOT NULL, -- 1 = Upvote, -1 = Downvote
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_PostVotes] PRIMARY KEY CLUSTERED ([VoteID] ASC),
        CONSTRAINT [FK_PostVotes_Posts] FOREIGN KEY([PostID]) REFERENCES [dbo].[Posts] ([PostID]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostVotes_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_PostVotes_UserPost] UNIQUE NONCLUSTERED ([PostID], [UserId]),
        CONSTRAINT [CK_PostVotes_Type] CHECK ([VoteType] IN (1, -1))
    )
    
    CREATE NONCLUSTERED INDEX [IX_PostVotes_PostID] ON [dbo].[PostVotes]([PostID], [VoteType])
    CREATE NONCLUSTERED INDEX [IX_PostVotes_UserID] ON [dbo].[PostVotes]([UserId])
    
    PRINT 'PostVotes table created'
END
GO

-- Comment Votes Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommentVotes')
BEGIN
    CREATE TABLE [dbo].[CommentVotes](
        [VoteID] [int] IDENTITY(1,1) NOT NULL,
        [CommentID] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [VoteType] [int] NOT NULL, -- 1 = Upvote, -1 = Downvote
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_CommentVotes] PRIMARY KEY CLUSTERED ([VoteID] ASC),
        CONSTRAINT [FK_CommentVotes_Comments] FOREIGN KEY([CommentID]) REFERENCES [dbo].[Comments] ([CommentID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommentVotes_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_CommentVotes_UserComment] UNIQUE NONCLUSTERED ([CommentID], [UserId]),
        CONSTRAINT [CK_CommentVotes_Type] CHECK ([VoteType] IN (1, -1))
    )
    
    CREATE NONCLUSTERED INDEX [IX_CommentVotes_CommentID] ON [dbo].[CommentVotes]([CommentID], [VoteType])
    CREATE NONCLUSTERED INDEX [IX_CommentVotes_UserID] ON [dbo].[CommentVotes]([UserId])
    
    PRINT 'CommentVotes table created'
END
GO

-- Forum Votes Table (for forum threads)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ForumVotes')
BEGIN
    CREATE TABLE [dbo].[ForumVotes](
        [VoteID] [int] IDENTITY(1,1) NOT NULL,
        [ForumThreadID] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [VoteType] [int] NOT NULL, -- 1 = Upvote, -1 = Downvote
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_ForumVotes] PRIMARY KEY CLUSTERED ([VoteID] ASC),
        CONSTRAINT [FK_ForumVotes_ForumThreads] FOREIGN KEY([ForumThreadID]) REFERENCES [dbo].[UsersFourm] ([UserFourmID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ForumVotes_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_ForumVotes_UserForum] UNIQUE NONCLUSTERED ([ForumThreadID], [UserId]),
        CONSTRAINT [CK_ForumVotes_Type] CHECK ([VoteType] IN (1, -1))
    )
    
    CREATE NONCLUSTERED INDEX [IX_ForumVotes_ForumID] ON [dbo].[ForumVotes]([ForumThreadID], [VoteType])
    CREATE NONCLUSTERED INDEX [IX_ForumVotes_UserID] ON [dbo].[ForumVotes]([UserId])
    
    PRINT 'ForumVotes table created'
END
GO

-- Social Shares Table (Track social media shares)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SocialShares')
BEGIN
    CREATE TABLE [dbo].[SocialShares](
        [ShareID] [int] IDENTITY(1,1) NOT NULL,
        [ContentType] [nvarchar](50) NOT NULL, -- post, comment, forum, blog
        [ContentID] [int] NOT NULL,
        [Platform] [nvarchar](50) NOT NULL, -- facebook, twitter, linkedin, reddit, whatsapp, telegram, email
        [SharedBy] [nvarchar](450) NULL, -- User who shared (nullable for anonymous shares)
        [IPAddress] [nvarchar](45) NULL,
        [UserAgent] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_SocialShares] PRIMARY KEY CLUSTERED ([ShareID] ASC),
        CONSTRAINT [FK_SocialShares_Users] FOREIGN KEY([SharedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [CK_SocialShares_ContentType] CHECK ([ContentType] IN ('post', 'comment', 'forum', 'blog', 'product')),
        CONSTRAINT [CK_SocialShares_Platform] CHECK ([Platform] IN ('facebook', 'twitter', 'linkedin', 'reddit', 'whatsapp', 'telegram', 'email', 'copy_link', 'other'))
    )
    
    CREATE NONCLUSTERED INDEX [IX_SocialShares_Content] ON [dbo].[SocialShares]([ContentType], [ContentID], [CreatedAt] DESC)
    CREATE NONCLUSTERED INDEX [IX_SocialShares_Platform] ON [dbo].[SocialShares]([Platform], [CreatedAt] DESC)
    CREATE NONCLUSTERED INDEX [IX_SocialShares_User] ON [dbo].[SocialShares]([SharedBy]) WHERE [SharedBy] IS NOT NULL
    
    PRINT 'SocialShares table created'
END
GO

-- Post Views Tracking (Detailed view analytics)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PostViews')
BEGIN
    CREATE TABLE [dbo].[PostViews](
        [ViewID] [bigint] IDENTITY(1,1) NOT NULL,
        [PostID] [int] NOT NULL,
        [UserId] [nvarchar](450) NULL,
        [IPAddress] [nvarchar](45) NULL,
        [UserAgent] [nvarchar](500) NULL,
        [Referrer] [nvarchar](500) NULL,
        [Country] [nvarchar](100) NULL,
        [City] [nvarchar](100) NULL,
        [DeviceType] [nvarchar](50) NULL, -- desktop, mobile, tablet
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_PostViews] PRIMARY KEY CLUSTERED ([ViewID] ASC),
        CONSTRAINT [FK_PostViews_Posts] FOREIGN KEY([PostID]) REFERENCES [dbo].[Posts] ([PostID]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostViews_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL
    )
    
    CREATE NONCLUSTERED INDEX [IX_PostViews_PostID] ON [dbo].[PostViews]([PostID], [CreatedAt] DESC)
    CREATE NONCLUSTERED INDEX [IX_PostViews_UserID] ON [dbo].[PostViews]([UserId]) WHERE [UserId] IS NOT NULL
    CREATE NONCLUSTERED INDEX [IX_PostViews_Date] ON [dbo].[PostViews]([CreatedAt] DESC)
    
    PRINT 'PostViews table created'
END
GO

-- Saved Posts (Bookmarks)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SavedPosts')
BEGIN
    CREATE TABLE [dbo].[SavedPosts](
        [SavedPostID] [int] IDENTITY(1,1) NOT NULL,
        [PostID] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_SavedPosts] PRIMARY KEY CLUSTERED ([SavedPostID] ASC),
        CONSTRAINT [FK_SavedPosts_Posts] FOREIGN KEY([PostID]) REFERENCES [dbo].[Posts] ([PostID]) ON DELETE CASCADE,
        CONSTRAINT [FK_SavedPosts_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_SavedPosts_UserPost] UNIQUE NONCLUSTERED ([PostID], [UserId])
    )
    
    CREATE NONCLUSTERED INDEX [IX_SavedPosts_UserID] ON [dbo].[SavedPosts]([UserId], [CreatedAt] DESC)
    CREATE NONCLUSTERED INDEX [IX_SavedPosts_PostID] ON [dbo].[SavedPosts]([PostID])
    
    PRINT 'SavedPosts table created'
END
GO

-- Post History (Track edits)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PostHistory')
BEGIN
    CREATE TABLE [dbo].[PostHistory](
        [HistoryID] [int] IDENTITY(1,1) NOT NULL,
        [PostID] [int] NOT NULL,
        [Title] [nvarchar](max) NULL,
        [Content] [nvarchar](max) NULL,
        [EditedBy] [nvarchar](450) NOT NULL,
        [EditReason] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_PostHistory] PRIMARY KEY CLUSTERED ([HistoryID] ASC),
        CONSTRAINT [FK_PostHistory_Posts] FOREIGN KEY([PostID]) REFERENCES [dbo].[Posts] ([PostID]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostHistory_Users] FOREIGN KEY([EditedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    )
    
    CREATE NONCLUSTERED INDEX [IX_PostHistory_PostID] ON [dbo].[PostHistory]([PostID], [CreatedAt] DESC)
    
    PRINT 'PostHistory table created'
END
GO

-- ================================================================
-- PART 4: CREATE TRIGGERS FOR AUTOMATIC COUNTS
-- ================================================================

-- Trigger to update Post vote counts
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TR' AND name = 'TR_UpdatePostVoteCounts')
    DROP TRIGGER [dbo].[TR_UpdatePostVoteCounts]
GO

CREATE TRIGGER [dbo].[TR_UpdatePostVoteCounts]
ON [dbo].[PostVotes]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON
    
    -- Update counts for affected posts
    UPDATE p
    SET 
        p.UpvoteCount = ISNULL((SELECT COUNT(*) FROM dbo.PostVotes WHERE PostID = p.PostID AND VoteType = 1), 0),
        p.DownvoteCount = ISNULL((SELECT COUNT(*) FROM dbo.PostVotes WHERE PostID = p.PostID AND VoteType = -1), 0),
        p.Score = ISNULL((SELECT COUNT(*) FROM dbo.PostVotes WHERE PostID = p.PostID AND VoteType = 1), 0) - 
                  ISNULL((SELECT COUNT(*) FROM dbo.PostVotes WHERE PostID = p.PostID AND VoteType = -1), 0)
    FROM dbo.Posts p
    WHERE p.PostID IN (
        SELECT PostID FROM inserted
        UNION
        SELECT PostID FROM deleted
    )
END
GO

-- Trigger to update Comment vote counts
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TR' AND name = 'TR_UpdateCommentVoteCounts')
    DROP TRIGGER [dbo].[TR_UpdateCommentVoteCounts]
GO

CREATE TRIGGER [dbo].[TR_UpdateCommentVoteCounts]
ON [dbo].[CommentVotes]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON
    
    UPDATE c
    SET 
        c.UpvoteCount = ISNULL((SELECT COUNT(*) FROM dbo.CommentVotes WHERE CommentID = c.CommentID AND VoteType = 1), 0),
        c.DownvoteCount = ISNULL((SELECT COUNT(*) FROM dbo.CommentVotes WHERE CommentID = c.CommentID AND VoteType = -1), 0)
    FROM dbo.Comments c
    WHERE c.CommentID IN (
        SELECT CommentID FROM inserted
        UNION
        SELECT CommentID FROM deleted
    )
END
GO

-- Trigger to update Comment count on Posts
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TR' AND name = 'TR_UpdatePostCommentCount')
    DROP TRIGGER [dbo].[TR_UpdatePostCommentCount]
GO

CREATE TRIGGER [dbo].[TR_UpdatePostCommentCount]
ON [dbo].[Comments]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON
    
    UPDATE p
    SET p.CommentCount = (
        SELECT COUNT(*) 
        FROM dbo.Comments c 
        WHERE c.PostID = p.PostID AND c.IsDeleted = 0
    )
    FROM dbo.Posts p
    WHERE p.PostID IN (
        SELECT PostID FROM inserted
        UNION
        SELECT PostID FROM deleted
    )
END
GO

PRINT 'Triggers created for automatic count updates'
GO

-- ================================================================
-- PART 5: CREATE VIEWS FOR ANALYTICS
-- ================================================================

-- Popular Posts View (SEO optimized)
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_PopularPosts')
    DROP VIEW [dbo].[vw_PopularPosts]
GO

CREATE VIEW [dbo].[vw_PopularPosts]
AS
SELECT 
    p.PostID,
    p.Title,
    p.Slug,
    p.Excerpt,
    p.MetaTitle,
    p.MetaDescription,
    p.FeaturedImage,
    p.Score,
    p.ViewCount,
    p.CommentCount,
    p.UpvoteCount,
    p.DownvoteCount,
    p.CreatedAt,
    p.PublishedAt,
    p.CommunityID,
    c.Name AS CommunityName,
    c.Slug AS CommunitySlug,
    u.UserName AS AuthorName,
    u.Id AS AuthorId,
    -- Calculate trending score (weighted algorithm)
    (p.Score * 2 + p.CommentCount * 3 + p.ViewCount * 0.1) AS TrendingScore
FROM dbo.Posts p
INNER JOIN dbo.Communities c ON p.CommunityID = c.CommunityID
INNER JOIN dbo.AspNetUsers u ON p.UserId = u.Id
WHERE p.PostStatus = 'published' 
    AND p.IsDeleted = 0
    AND p.PublishedAt IS NOT NULL
GO

-- Post Analytics View
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_PostAnalytics')
    DROP VIEW [dbo].[vw_PostAnalytics]
GO

CREATE VIEW [dbo].[vw_PostAnalytics]
AS
SELECT 
    p.PostID,
    p.Title,
    p.ViewCount,
    p.CommentCount,
    p.UpvoteCount,
    p.DownvoteCount,
    p.Score,
    (SELECT COUNT(*) FROM dbo.SocialShares WHERE ContentType = 'post' AND ContentID = p.PostID) AS ShareCount,
    (SELECT COUNT(*) FROM dbo.SavedPosts WHERE PostID = p.PostID) AS SaveCount,
    (SELECT COUNT(*) FROM dbo.PostViews WHERE PostID = p.PostID AND CreatedAt >= DATEADD(day, -7, GETUTCDATE())) AS ViewsLast7Days,
    (SELECT COUNT(*) FROM dbo.PostViews WHERE PostID = p.PostID AND CreatedAt >= DATEADD(day, -30, GETUTCDATE())) AS ViewsLast30Days,
    p.CreatedAt,
    p.PublishedAt
FROM dbo.Posts p
WHERE p.IsDeleted = 0
GO

PRINT 'Analytics views created'
GO

-- ================================================================
-- PART 6: STORED PROCEDURES FOR COMMON OPERATIONS
-- ================================================================

-- Procedure to vote on a post
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_VotePost')
    DROP PROCEDURE [dbo].[sp_VotePost]
GO

CREATE PROCEDURE [dbo].[sp_VotePost]
    @PostID INT,
    @UserID NVARCHAR(450),
    @VoteType INT -- 1 for upvote, -1 for downvote, 0 to remove vote
AS
BEGIN
    SET NOCOUNT ON
    
    BEGIN TRANSACTION
    
    BEGIN TRY
        IF @VoteType = 0
        BEGIN
            -- Remove existing vote
            DELETE FROM dbo.PostVotes WHERE PostID = @PostID AND UserId = @UserID
        END
        ELSE
        BEGIN
            -- Insert or update vote
            MERGE dbo.PostVotes AS target
            USING (SELECT @PostID AS PostID, @UserID AS UserId, @VoteType AS VoteType) AS source
            ON target.PostID = source.PostID AND target.UserId = source.UserId
            WHEN MATCHED THEN
                UPDATE SET VoteType = source.VoteType, UpdatedAt = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (PostID, UserId, VoteType) VALUES (source.PostID, source.UserId, source.VoteType);
        END
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

-- Procedure to get user's vote on a post
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetUserPostVote')
    DROP PROCEDURE [dbo].[sp_GetUserPostVote]
GO

CREATE PROCEDURE [dbo].[sp_GetUserPostVote]
    @PostID INT,
    @UserID NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON
    
    SELECT VoteType 
    FROM dbo.PostVotes 
    WHERE PostID = @PostID AND UserId = @UserID
END
GO

PRINT 'Stored procedures created'
GO

-- ================================================================
-- PART 7: UPDATE EXISTING DATA
-- ================================================================

-- Initialize vote counts from Reactions table (if exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Reactions')
BEGIN
    -- Migrate reactions to votes (if needed)
    -- This is optional - you may want to keep Reactions table for compatibility
    
    PRINT 'Reactions table exists - consider migrating to PostVotes'
END
GO

-- Update Posts with default values (only update columns that exist and need initialization)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Posts')
BEGIN
    -- Update Score if column exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Score')
    BEGIN
        UPDATE [dbo].[Posts] SET Score = ISNULL(Score, 0) WHERE Score IS NULL
    END
    
    -- Update CommentCount if column exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'CommentCount')
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'IsDeleted')
            UPDATE [dbo].[Posts] SET CommentCount = (SELECT COUNT(*) FROM dbo.Comments WHERE PostID = Posts.PostID AND IsDeleted = 0) WHERE CommentCount IS NULL
        ELSE
            UPDATE [dbo].[Posts] SET CommentCount = (SELECT COUNT(*) FROM dbo.Comments WHERE PostID = Posts.PostID) WHERE CommentCount IS NULL
    END
    
    -- Update other new columns with defaults
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'UpvoteCount')
        UPDATE [dbo].[Posts] SET UpvoteCount = ISNULL(UpvoteCount, 0) WHERE UpvoteCount IS NULL
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'DownvoteCount')
        UPDATE [dbo].[Posts] SET DownvoteCount = ISNULL(DownvoteCount, 0) WHERE DownvoteCount IS NULL
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsLocked')
        UPDATE [dbo].[Posts] SET IsLocked = ISNULL(IsLocked, 0) WHERE IsLocked IS NULL
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsPinned')
        UPDATE [dbo].[Posts] SET IsPinned = ISNULL(IsPinned, 0) WHERE IsPinned IS NULL
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'IsDeleted')
        UPDATE [dbo].[Posts] SET IsDeleted = ISNULL(IsDeleted, 0) WHERE IsDeleted IS NULL
END

PRINT 'Existing data updated with defaults'
GO

-- ================================================================
-- COMPLETION MESSAGE
-- ================================================================
PRINT '========================================'
PRINT 'Database modernization completed!'
PRINT '========================================'
PRINT 'New tables created:'
PRINT '  - PostReports (content reporting)'
PRINT '  - CommentReports (comment reporting)'
PRINT '  - UserBlocks (user blocking system)'
PRINT '  - PostVotes (modern voting system)'
PRINT '  - CommentVotes (comment voting)'
PRINT '  - ForumVotes (forum voting)'
PRINT '  - SocialShares (social media tracking)'
PRINT '  - PostViews (detailed view analytics)'
PRINT '  - SavedPosts (bookmarks)'
PRINT '  - PostHistory (edit tracking)'
PRINT ''
PRINT 'Tables modernized with:'
PRINT '  - SEO fields (CanonicalUrl, FocusKeyword, SchemaMarkup)'
PRINT '  - Performance indexes'
PRINT '  - Audit fields'
PRINT '  - Soft delete support'
PRINT ''
PRINT 'All gsmsharing schema tables removed'
PRINT '========================================'
GO

