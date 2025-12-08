-- =============================================
-- GSMSharing V2 - Database Improvements Script
-- Based on actual database structure from db.sql
-- Purpose: Add missing foreign keys, indexes, constraints
-- Priority: CRITICAL - Run before Phase 1
-- Database: gsmsharingv3 (or gsmsharingv2)
-- =============================================

USE gsmsharingv3;  -- Change to gsmsharingv2 if needed
GO

PRINT '========================================';
PRINT 'GSMSharing V2 - Database Improvements';
PRINT 'Based on actual database structure';
PRINT 'Starting improvements...';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. FOREIGN KEY CONSTRAINTS
-- =============================================
PRINT '1. Adding Foreign Key Constraints...';
PRINT '----------------------------------------';

-- =============================================
-- NEW TABLES (Posts, Communities, Comments, Categories)
-- These may not exist yet - check first
-- =============================================

-- Posts Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Posts_AspNetUsers')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT FK_Posts_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE SET NULL;
        PRINT '✅ Created FK_Posts_AspNetUsers';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Posts_Communities')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT FK_Posts_Communities FOREIGN KEY (CommunityID) 
        REFERENCES Communities(CommunityID) ON DELETE SET NULL;
        PRINT '✅ Created FK_Posts_Communities';
    END
END

-- Comments Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_Posts')
    BEGIN
        ALTER TABLE Comments
        ADD CONSTRAINT FK_Comments_Posts FOREIGN KEY (PostID) 
        REFERENCES Posts(PostID) ON DELETE CASCADE;
        PRINT '✅ Created FK_Comments_Posts';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_AspNetUsers')
    BEGIN
        ALTER TABLE Comments
        ADD CONSTRAINT FK_Comments_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE SET NULL;
        PRINT '✅ Created FK_Comments_AspNetUsers';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_ParentComment')
    BEGIN
        ALTER TABLE Comments
        ADD CONSTRAINT FK_Comments_ParentComment FOREIGN KEY (ParentCommentID) 
        REFERENCES Comments(CommentID) ON DELETE NO ACTION;
        PRINT '✅ Created FK_Comments_ParentComment';
    END
END

-- Communities Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Communities' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Communities_AspNetUsers')
    BEGIN
        ALTER TABLE Communities
        ADD CONSTRAINT FK_Communities_AspNetUsers FOREIGN KEY (CreatorId) 
        REFERENCES AspNetUsers(Id) ON DELETE SET NULL;
        PRINT '✅ Created FK_Communities_AspNetUsers';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Communities_Categories')
    BEGIN
        ALTER TABLE Communities
        ADD CONSTRAINT FK_Communities_Categories FOREIGN KEY (CategoryID) 
        REFERENCES Categories(CategoryID) ON DELETE SET NULL;
        PRINT '✅ Created FK_Communities_Categories';
    END
END

-- Categories Self-Reference (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Categories_ParentCategory')
    BEGIN
        ALTER TABLE Categories
        ADD CONSTRAINT FK_Categories_ParentCategory FOREIGN KEY (ParentCategoryID) 
        REFERENCES Categories(CategoryID) ON DELETE NO ACTION;
        PRINT '✅ Created FK_Categories_ParentCategory';
    END
END

-- UserProfiles Foreign Key (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserProfiles' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserProfiles_AspNetUsers')
    BEGIN
        ALTER TABLE UserProfiles
        ADD CONSTRAINT FK_UserProfiles_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
        PRINT '✅ Created FK_UserProfiles_AspNetUsers';
    END
END

-- CommunityMembers Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunityMembers' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CommunityMembers_Communities')
    BEGIN
        ALTER TABLE CommunityMembers
        ADD CONSTRAINT FK_CommunityMembers_Communities FOREIGN KEY (CommunityID) 
        REFERENCES Communities(CommunityID) ON DELETE CASCADE;
        PRINT '✅ Created FK_CommunityMembers_Communities';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CommunityMembers_AspNetUsers')
    BEGIN
        ALTER TABLE CommunityMembers
        ADD CONSTRAINT FK_CommunityMembers_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
        PRINT '✅ Created FK_CommunityMembers_AspNetUsers';
    END
END

-- Reactions Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reactions' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reactions_AspNetUsers')
    BEGIN
        ALTER TABLE Reactions
        ADD CONSTRAINT FK_Reactions_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
        PRINT '✅ Created FK_Reactions_AspNetUsers';
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reactions_Posts')
        BEGIN
            ALTER TABLE Reactions
            ADD CONSTRAINT FK_Reactions_Posts FOREIGN KEY (PostID) 
            REFERENCES Posts(PostID) ON DELETE CASCADE;
            PRINT '✅ Created FK_Reactions_Posts';
        END
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reactions_Comments')
        BEGIN
            ALTER TABLE Reactions
            ADD CONSTRAINT FK_Reactions_Comments FOREIGN KEY (CommentID) 
            REFERENCES Comments(CommentID) ON DELETE CASCADE;
            PRINT '✅ Created FK_Reactions_Comments';
        END
    END
END

-- PostTags Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PostTags' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PostTags_Posts')
        BEGIN
            ALTER TABLE PostTags
            ADD CONSTRAINT FK_PostTags_Posts FOREIGN KEY (PostID) 
            REFERENCES Posts(PostID) ON DELETE CASCADE;
            PRINT '✅ Created FK_PostTags_Posts';
        END
    END

    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tags' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PostTags_Tags')
        BEGIN
            ALTER TABLE PostTags
            ADD CONSTRAINT FK_PostTags_Tags FOREIGN KEY (TagID) 
            REFERENCES Tags(TagID) ON DELETE CASCADE;
            PRINT '✅ Created FK_PostTags_Tags';
        END
    END
END

-- ChatRooms Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatRooms' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Communities' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRooms_Communities')
        BEGIN
            ALTER TABLE ChatRooms
            ADD CONSTRAINT FK_ChatRooms_Communities FOREIGN KEY (CommunityID) 
            REFERENCES Communities(CommunityID) ON DELETE SET NULL;
            PRINT '✅ Created FK_ChatRooms_Communities';
        END
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRooms_AspNetUsers')
    BEGIN
        ALTER TABLE ChatRooms
        ADD CONSTRAINT FK_ChatRooms_AspNetUsers FOREIGN KEY (CreatedBy) 
        REFERENCES AspNetUsers(Id) ON DELETE SET NULL;
        PRINT '✅ Created FK_ChatRooms_AspNetUsers';
    END
END

-- ChatRoomMembers Foreign Keys (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatRoomMembers' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatRooms' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRoomMembers_ChatRooms')
        BEGIN
            ALTER TABLE ChatRoomMembers
            ADD CONSTRAINT FK_ChatRoomMembers_ChatRooms FOREIGN KEY (RoomID) 
            REFERENCES ChatRooms(RoomID) ON DELETE CASCADE;
            PRINT '✅ Created FK_ChatRoomMembers_ChatRooms';
        END
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRoomMembers_AspNetUsers')
    BEGIN
        ALTER TABLE ChatRoomMembers
        ADD CONSTRAINT FK_ChatRoomMembers_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
        PRINT '✅ Created FK_ChatRoomMembers_AspNetUsers';
    END
END

-- Notifications Foreign Key (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_AspNetUsers')
    BEGIN
        ALTER TABLE Notifications
        ADD CONSTRAINT FK_Notifications_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
        PRINT '✅ Created FK_Notifications_AspNetUsers';
    END
END

-- =============================================
-- EXISTING TABLES FROM db.sql
-- These tables already exist and have some FKs
-- =============================================

-- MobileAds - FK already exists (FK_AdsU), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileAds' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AdsU')
    BEGIN
        ALTER TABLE MobileAds
        ADD CONSTRAINT FK_AdsU FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_AdsU (MobileAds)';
    END
END

-- MobilePartAds - FK already exists (FK_AdsMobAd), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobilePartAds' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AdsMobAd')
    BEGIN
        ALTER TABLE MobilePartAds
        ADD CONSTRAINT FK_AdsMobAd FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_AdsMobAd (MobilePartAds)';
    END
END

-- MobileSpecs - FK already exists (FK_SpecUser), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileSpecs' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SpecUser')
    BEGIN
        ALTER TABLE MobileSpecs
        ADD CONSTRAINT FK_SpecUser FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_SpecUser (MobileSpecs)';
    END
END

-- UsersFourm (Forum) - FK already exists (FK_UFourm), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsersFourm' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UFourm')
    BEGIN
        ALTER TABLE UsersFourm
        ADD CONSTRAINT FK_UFourm FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_UFourm (UsersFourm)';
    END
END

-- ForumReplys - FKs already exist, but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ForumReplys' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'Reply')
    BEGIN
        ALTER TABLE ForumReplys
        ADD CONSTRAINT Reply FOREIGN KEY (ThreadId) 
        REFERENCES UsersFourm(UserFourmID);
        PRINT '✅ Created Reply FK (ForumReplys)';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'UserFK')
    BEGIN
        ALTER TABLE ForumReplys
        ADD CONSTRAINT UserFK FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created UserFK (ForumReplys)';
    END
END

-- ForumCategory - FK already exists (FK_CUFourm), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ForumCategory' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CUFourm')
    BEGIN
        ALTER TABLE ForumCategory
        ADD CONSTRAINT FK_CUFourm FOREIGN KEY (UserFourmID) 
        REFERENCES UsersFourm(UserFourmID);
        PRINT '✅ Created FK_CUFourm (ForumCategory)';
    END
END

-- FourmComments - FK already exists (FK_FComment), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FourmComments' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_FComment')
    BEGIN
        ALTER TABLE FourmComments
        ADD CONSTRAINT FK_FComment FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_FComment (FourmComments)';
    END
END

-- AdsImage - FK already exists (FK_AdsImage_MobileAds), but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AdsImage' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AdsImage_MobileAds')
    BEGIN
        ALTER TABLE AdsImage
        ADD CONSTRAINT FK_AdsImage_MobileAds FOREIGN KEY (AdsId) 
        REFERENCES MobileAds(AdsId);
        PRINT '✅ Created FK_AdsImage_MobileAds';
    END
END

-- SocialCommunities - FK exists but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SocialCommunities' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SocialCommunities_SocialCategories')
    BEGIN
        ALTER TABLE SocialCommunities
        ADD CONSTRAINT FK_SocialCommunities_SocialCategories FOREIGN KEY (CategoryId) 
        REFERENCES SocialCategories(CategoryId);
        PRINT '✅ Created FK_SocialCommunities_SocialCategories';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SocialCommunities_AspNetUsers')
    BEGIN
        ALTER TABLE SocialCommunities
        ADD CONSTRAINT FK_SocialCommunities_AspNetUsers FOREIGN KEY (CreatedBy) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_SocialCommunities_AspNetUsers';
    END
END

-- Users_Communities - FKs exist but verify
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users_Communities' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Communities_SocialCommunities')
    BEGIN
        ALTER TABLE Users_Communities
        ADD CONSTRAINT FK_Users_Communities_SocialCommunities FOREIGN KEY (CommunityId) 
        REFERENCES SocialCommunities(CommunityId);
        PRINT '✅ Created FK_Users_Communities_SocialCommunities';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Communities_AspNetUsers')
    BEGIN
        ALTER TABLE Users_Communities
        ADD CONSTRAINT FK_Users_Communities_AspNetUsers FOREIGN KEY (UserId) 
        REFERENCES AspNetUsers(Id);
        PRINT '✅ Created FK_Users_Communities_AspNetUsers';
    END
END

PRINT '';

-- =============================================
-- 2. INDEXES
-- =============================================
PRINT '2. Adding Indexes...';
PRINT '----------------------------------------';

-- Posts Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_UserId' AND object_id = OBJECT_ID('dbo.Posts'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_UserId ON Posts(UserId);
        PRINT '✅ Created IX_Posts_UserId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_CommunityID' AND object_id = OBJECT_ID('dbo.Posts'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_CommunityID ON Posts(CommunityID);
        PRINT '✅ Created IX_Posts_CommunityID';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_PostStatus' AND object_id = OBJECT_ID('dbo.Posts'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_PostStatus ON Posts(PostStatus);
        PRINT '✅ Created IX_Posts_PostStatus';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_CreatedAt' AND object_id = OBJECT_ID('dbo.Posts'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_CreatedAt ON Posts(CreatedAt DESC);
        PRINT '✅ Created IX_Posts_CreatedAt';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_Slug' AND object_id = OBJECT_ID('dbo.Posts'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Posts_Slug ON Posts(Slug) WHERE Slug IS NOT NULL;
        PRINT '✅ Created IX_Posts_Slug';
    END
END

-- Comments Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_PostID' AND object_id = OBJECT_ID('dbo.Comments'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Comments_PostID ON Comments(PostID);
        PRINT '✅ Created IX_Comments_PostID';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_UserId' AND object_id = OBJECT_ID('dbo.Comments'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Comments_UserId ON Comments(UserId);
        PRINT '✅ Created IX_Comments_UserId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_ParentCommentID' AND object_id = OBJECT_ID('dbo.Comments'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Comments_ParentCommentID ON Comments(ParentCommentID);
        PRINT '✅ Created IX_Comments_ParentCommentID';
    END
END

-- Communities Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Communities' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Communities_Slug' AND object_id = OBJECT_ID('dbo.Communities'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_Communities_Slug ON Communities(Slug) WHERE Slug IS NOT NULL;
        PRINT '✅ Created IX_Communities_Slug (UNIQUE)';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Communities_CreatorId' AND object_id = OBJECT_ID('dbo.Communities'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Communities_CreatorId ON Communities(CreatorId);
        PRINT '✅ Created IX_Communities_CreatorId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Communities_CategoryID' AND object_id = OBJECT_ID('dbo.Communities'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Communities_CategoryID ON Communities(CategoryID);
        PRINT '✅ Created IX_Communities_CategoryID';
    END
END

-- Categories Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Categories_ParentCategoryID' AND object_id = OBJECT_ID('dbo.Categories'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Categories_ParentCategoryID ON Categories(ParentCategoryID);
        PRINT '✅ Created IX_Categories_ParentCategoryID';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Categories_Slug' AND object_id = OBJECT_ID('dbo.Categories'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_Categories_Slug ON Categories(Slug) WHERE Slug IS NOT NULL;
        PRINT '✅ Created IX_Categories_Slug (UNIQUE)';
    END
END

-- Tags Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tags' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tags_Slug' AND object_id = OBJECT_ID('dbo.Tags'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_Tags_Slug ON Tags(Slug) WHERE Slug IS NOT NULL;
        PRINT '✅ Created IX_Tags_Slug (UNIQUE)';
    END
END

-- UserProfiles Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserProfiles' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserProfiles_UserId' AND object_id = OBJECT_ID('dbo.UserProfiles'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_UserProfiles_UserId ON UserProfiles(UserId) WHERE UserId IS NOT NULL;
        PRINT '✅ Created IX_UserProfiles_UserId (UNIQUE)';
    END
END

-- CommunityMembers Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunityMembers' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CommunityMembers_CommunityID' AND object_id = OBJECT_ID('dbo.CommunityMembers'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_CommunityMembers_CommunityID ON CommunityMembers(CommunityID);
        PRINT '✅ Created IX_CommunityMembers_CommunityID';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CommunityMembers_UserId' AND object_id = OBJECT_ID('dbo.CommunityMembers'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_CommunityMembers_UserId ON CommunityMembers(UserId);
        PRINT '✅ Created IX_CommunityMembers_UserId';
    END
END

-- Reactions Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reactions' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reactions_PostID' AND object_id = OBJECT_ID('dbo.Reactions'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Reactions_PostID ON Reactions(PostID);
        PRINT '✅ Created IX_Reactions_PostID';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reactions_CommentID' AND object_id = OBJECT_ID('dbo.Reactions'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Reactions_CommentID ON Reactions(CommentID);
        PRINT '✅ Created IX_Reactions_CommentID';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reactions_UserId' AND object_id = OBJECT_ID('dbo.Reactions'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Reactions_UserId ON Reactions(UserId);
        PRINT '✅ Created IX_Reactions_UserId';
    END
END

-- Notifications Indexes (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_UserId' AND object_id = OBJECT_ID('dbo.Notifications'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Notifications_UserId ON Notifications(UserId);
        PRINT '✅ Created IX_Notifications_UserId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_IsRead' AND object_id = OBJECT_ID('dbo.Notifications'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Notifications_IsRead ON Notifications(IsRead) WHERE IsRead = 0;
        PRINT '✅ Created IX_Notifications_IsRead';
    END
END

-- MobileAds Indexes (existing table from db.sql)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileAds' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileAds_UserId' AND object_id = OBJECT_ID('dbo.MobileAds'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobileAds_UserId ON MobileAds(UserId);
        PRINT '✅ Created IX_MobileAds_UserId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileAds_Publish' AND object_id = OBJECT_ID('dbo.MobileAds'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobileAds_Publish ON MobileAds(publish);
        PRINT '✅ Created IX_MobileAds_Publish';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileAds_CreationDate' AND object_id = OBJECT_ID('dbo.MobileAds'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobileAds_CreationDate ON MobileAds(CreationDate DESC);
        PRINT '✅ Created IX_MobileAds_CreationDate';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileAds_Price' AND object_id = OBJECT_ID('dbo.MobileAds'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobileAds_Price ON MobileAds(price) WHERE price IS NOT NULL;
        PRINT '✅ Created IX_MobileAds_Price';
    END
END

-- MobilePartAds Indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobilePartAds' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobilePartAds_UserId' AND object_id = OBJECT_ID('dbo.MobilePartAds'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobilePartAds_UserId ON MobilePartAds(UserId);
        PRINT '✅ Created IX_MobilePartAds_UserId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobilePartAds_Publish' AND object_id = OBJECT_ID('dbo.MobilePartAds'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobilePartAds_Publish ON MobilePartAds(publish);
        PRINT '✅ Created IX_MobilePartAds_Publish';
    END
END

-- MobileSpecs Indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileSpecs' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileSpecs_ModelName' AND object_id = OBJECT_ID('dbo.MobileSpecs'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobileSpecs_ModelName ON MobileSpecs(ModelName);
        PRINT '✅ Created IX_MobileSpecs_ModelName';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileSpecs_UserId' AND object_id = OBJECT_ID('dbo.MobileSpecs'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_MobileSpecs_UserId ON MobileSpecs(UserId);
        PRINT '✅ Created IX_MobileSpecs_UserId';
    END
END

-- UsersFourm (Forum) Indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsersFourm' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsersFourm_UserId' AND object_id = OBJECT_ID('dbo.UsersFourm'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_UsersFourm_UserId ON UsersFourm(UserId);
        PRINT '✅ Created IX_UsersFourm_UserId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsersFourm_Publish' AND object_id = OBJECT_ID('dbo.UsersFourm'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_UsersFourm_Publish ON UsersFourm(publish);
        PRINT '✅ Created IX_UsersFourm_Publish';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsersFourm_CreationDate' AND object_id = OBJECT_ID('dbo.UsersFourm'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_UsersFourm_CreationDate ON UsersFourm(CreationDate DESC);
        PRINT '✅ Created IX_UsersFourm_CreationDate';
    END
END

-- ForumReplys Indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ForumReplys' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ForumReplys_ThreadId' AND object_id = OBJECT_ID('dbo.ForumReplys'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_ForumReplys_ThreadId ON ForumReplys(ThreadId);
        PRINT '✅ Created IX_ForumReplys_ThreadId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ForumReplys_UserId' AND object_id = OBJECT_ID('dbo.ForumReplys'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_ForumReplys_UserId ON ForumReplys(UserId);
        PRINT '✅ Created IX_ForumReplys_UserId';
    END
END

-- SocialCommunities Indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SocialCommunities' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SocialCommunities_CategoryId' AND object_id = OBJECT_ID('dbo.SocialCommunities'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SocialCommunities_CategoryId ON SocialCommunities(CategoryId);
        PRINT '✅ Created IX_SocialCommunities_CategoryId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SocialCommunities_CreatedBy' AND object_id = OBJECT_ID('dbo.SocialCommunities'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SocialCommunities_CreatedBy ON SocialCommunities(CreatedBy);
        PRINT '✅ Created IX_SocialCommunities_CreatedBy';
    END
END

-- Users_Communities Indexes
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users_Communities' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Communities_CommunityId' AND object_id = OBJECT_ID('dbo.Users_Communities'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Users_Communities_CommunityId ON Users_Communities(CommunityId);
        PRINT '✅ Created IX_Users_Communities_CommunityId';
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Communities_UserId' AND object_id = OBJECT_ID('dbo.Users_Communities'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Users_Communities_UserId ON Users_Communities(UserId);
        PRINT '✅ Created IX_Users_Communities_UserId';
    END
END

PRINT '';

-- =============================================
-- 3. CHECK CONSTRAINTS
-- =============================================
PRINT '3. Adding Check Constraints...';
PRINT '----------------------------------------';

-- Posts Check Constraints (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Posts_ViewCount')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT CK_Posts_ViewCount CHECK (ViewCount >= 0);
        PRINT '✅ Created CK_Posts_ViewCount';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Posts_PostStatus')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT CK_Posts_PostStatus CHECK (PostStatus IN ('Draft', 'Published', 'Archived', 'published', 'draft', 'archived') OR PostStatus IS NULL);
        PRINT '✅ Created CK_Posts_PostStatus';
    END
END

-- MobileAds Check Constraints
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileAds' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobileAds_Price')
    BEGIN
        ALTER TABLE MobileAds
        ADD CONSTRAINT CK_MobileAds_Price CHECK (price >= 0 OR price IS NULL);
        PRINT '✅ Created CK_MobileAds_Price';
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobileAds_Publish')
    BEGIN
        ALTER TABLE MobileAds
        ADD CONSTRAINT CK_MobileAds_Publish CHECK (publish IN (0, 1) OR publish IS NULL);
        PRINT '✅ Created CK_MobileAds_Publish';
    END
END

-- MobilePartAds Check Constraints
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobilePartAds' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobilePartAds_Price')
    BEGIN
        ALTER TABLE MobilePartAds
        ADD CONSTRAINT CK_MobilePartAds_Price CHECK (price >= 0 OR price IS NULL);
        PRINT '✅ Created CK_MobilePartAds_Price';
    END
END

PRINT '';

-- =============================================
-- 4. DEFAULT VALUES
-- =============================================
PRINT '4. Adding Default Values...';
PRINT '----------------------------------------';

-- Posts Defaults (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Posts_ViewCount')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT DF_Posts_ViewCount DEFAULT 0 FOR ViewCount;
        PRINT '✅ Created DF_Posts_ViewCount';
    END

    IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Posts_AllowComments')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT DF_Posts_AllowComments DEFAULT 1 FOR AllowComments;
        PRINT '✅ Created DF_Posts_AllowComments';
    END

    IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Posts_CreatedAt')
    BEGIN
        ALTER TABLE Posts
        ADD CONSTRAINT DF_Posts_CreatedAt DEFAULT GETUTCDATE() FOR CreatedAt;
        PRINT '✅ Created DF_Posts_CreatedAt';
    END
END

-- Comments Defaults (if table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments' AND TABLE_SCHEMA = 'dbo')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Comments_IsApproved')
    BEGIN
        ALTER TABLE Comments
        ADD CONSTRAINT DF_Comments_IsApproved DEFAULT 1 FOR IsApproved;
        PRINT '✅ Created DF_Comments_IsApproved';
    END

    IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Comments_CreatedAt')
    BEGIN
        ALTER TABLE Comments
        ADD CONSTRAINT DF_Comments_CreatedAt DEFAULT GETUTCDATE() FOR CreatedAt;
        PRINT '✅ Created DF_Comments_CreatedAt';
    END
END

PRINT '';

-- =============================================
-- COMPLETION
-- =============================================
PRINT '========================================';
PRINT 'Database Improvements Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '- Foreign Keys: Added/Verified';
PRINT '- Indexes: Added';
PRINT '- Check Constraints: Added';
PRINT '- Default Values: Added';
PRINT '';
PRINT 'Note:';
PRINT '- Script checks for table existence before adding constraints';
PRINT '- Works with both new tables (Posts, Communities) and existing tables';
PRINT '- Some foreign keys may already exist from db.sql';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Verify all improvements were applied';
PRINT '2. Test application';
PRINT '3. Check query performance';
PRINT '4. Proceed to Phase 1';
PRINT '';
