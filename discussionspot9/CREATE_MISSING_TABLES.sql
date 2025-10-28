-- ================================================================
-- Create All Missing Tables for Notification & Report System
-- Run this script to fix: "Invalid object name 'PostReports'" and "Invalid object name 'EmailQueues'"
-- ================================================================

USE DiscussionspotADO;
GO

PRINT '========================================';
PRINT 'Creating Missing Tables...';
PRINT '========================================';
GO

-- ================================================================
-- 1. POST REPORTS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostReports]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostReports] (
        [ReportId] INT PRIMARY KEY IDENTITY(1,1),
        [PostId] INT NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Reason] NVARCHAR(200) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ReviewedAt] DATETIME2 NULL,
        [ReviewedByUserId] NVARCHAR(450) NULL,
        [AdminNotes] NVARCHAR(MAX) NULL,
        
        -- Foreign Keys
        CONSTRAINT [FK_PostReports_Posts] 
            FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostReports_User] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PostReports_ReviewedBy] 
            FOREIGN KEY ([ReviewedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION
    );
    
    -- Create indexes
    CREATE INDEX IX_PostReports_Status_CreatedAt ON [dbo].[PostReports]([Status], [CreatedAt] DESC);
    CREATE INDEX IX_PostReports_PostId ON [dbo].[PostReports]([PostId]);
    CREATE INDEX IX_PostReports_UserId ON [dbo].[PostReports]([UserId]);
    
    PRINT '✅ Created PostReports table';
END
ELSE
BEGIN
    PRINT '⚠️ PostReports table already exists';
END
GO

-- ================================================================
-- 2. EMAIL QUEUE TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailQueues]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EmailQueues] (
        [EmailQueueId] INT PRIMARY KEY IDENTITY(1,1),
        [ToEmail] NVARCHAR(256) NOT NULL,
        [ToName] NVARCHAR(100),
        [Subject] NVARCHAR(200) NOT NULL,
        [HtmlBody] NVARCHAR(MAX) NOT NULL,
        [PlainTextBody] NVARCHAR(MAX),
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending',
        [Priority] INT NOT NULL DEFAULT 5,
        [Attempts] INT NOT NULL DEFAULT 0,
        [MaxAttempts] INT NOT NULL DEFAULT 3,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ScheduledFor] DATETIME2 NULL,
        [SentAt] DATETIME2 NULL,
        [ErrorMessage] NVARCHAR(MAX),
        [EmailType] NVARCHAR(50),
        [UserId] NVARCHAR(450),
        [NotificationId] INT,
        
        CONSTRAINT CK_EmailQueues_Status CHECK (Status IN ('pending', 'sending', 'sent', 'failed')),
        CONSTRAINT CK_EmailQueues_Priority CHECK (Priority BETWEEN 1 AND 10)
    );
    
    -- Create indexes
    CREATE INDEX IX_EmailQueues_Status_Priority ON [dbo].[EmailQueues]([Status], [Priority]);
    CREATE INDEX IX_EmailQueues_ScheduledFor ON [dbo].[EmailQueues]([ScheduledFor]) WHERE [ScheduledFor] IS NOT NULL;
    CREATE INDEX IX_EmailQueues_UserId ON [dbo].[EmailQueues]([UserId]) WHERE [UserId] IS NOT NULL;
    CREATE INDEX IX_EmailQueues_CreatedAt ON [dbo].[EmailQueues]([CreatedAt]);
    
    PRINT '✅ Created EmailQueues table';
END
ELSE
BEGIN
    PRINT '⚠️ EmailQueues table already exists';
END
GO

-- ================================================================
-- 3. NOTIFICATION PREFERENCES TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NotificationPreferences] (
        [PreferenceId] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] NVARCHAR(450) NOT NULL,
        [NotificationType] NVARCHAR(50) NOT NULL,
        [WebEnabled] BIT NOT NULL DEFAULT 1,
        [EmailEnabled] BIT NOT NULL DEFAULT 1,
        [PushEnabled] BIT NOT NULL DEFAULT 0,
        [EmailFrequency] NVARCHAR(20) NOT NULL DEFAULT 'instant',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_NotificationPreferences_User 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT UQ_NotificationPreferences_UserId_Type UNIQUE ([UserId], [NotificationType]),
        CONSTRAINT CK_NotificationPreferences_EmailFrequency 
            CHECK (EmailFrequency IN ('instant', 'hourly', 'daily', 'weekly', 'never'))
    );
    
    -- Create indexes
    CREATE INDEX IX_NotificationPreferences_UserId ON [dbo].[NotificationPreferences]([UserId]);
    CREATE INDEX IX_NotificationPreferences_Type ON [dbo].[NotificationPreferences]([NotificationType]);
    
    PRINT '✅ Created NotificationPreferences table';
END
ELSE
BEGIN
    PRINT '⚠️ NotificationPreferences table already exists';
END
GO

-- ================================================================
-- 4. USER NOTIFICATION SETTINGS TABLE
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserNotificationSettings] (
        [UserId] NVARCHAR(450) PRIMARY KEY,
        [EmailNotificationsEnabled] BIT NOT NULL DEFAULT 1,
        [WebNotificationsEnabled] BIT NOT NULL DEFAULT 1,
        [PushNotificationsEnabled] BIT NOT NULL DEFAULT 0,
        [EmailDigestFrequency] NVARCHAR(20) NOT NULL DEFAULT 'never',
        [QuietHoursEnabled] BIT NOT NULL DEFAULT 0,
        [QuietHoursStart] TIME NULL,
        [QuietHoursEnd] TIME NULL,
        [GroupNotifications] BIT NOT NULL DEFAULT 1,
        [ShowNotificationPreviews] BIT NOT NULL DEFAULT 1,
        [PlayNotificationSound] BIT NOT NULL DEFAULT 0,
        [UnsubscribeFromAll] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_UserNotificationSettings_User 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT CK_UserNotificationSettings_EmailDigestFrequency 
            CHECK (EmailDigestFrequency IN ('never', 'daily', 'weekly'))
    );
    
    PRINT '✅ Created UserNotificationSettings table';
END
ELSE
BEGIN
    PRINT '⚠️ UserNotificationSettings table already exists';
END
GO

-- ================================================================
-- 5. USER FOLLOWS TABLE (if missing)
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFollows]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserFollows] (
        [FollowId] INT PRIMARY KEY IDENTITY(1,1),
        [FollowerId] NVARCHAR(450) NOT NULL,
        [FollowedId] NVARCHAR(450) NOT NULL,
        [FollowedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [NotificationsEnabled] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT FK_UserFollows_Follower 
            FOREIGN KEY ([FollowerId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT FK_UserFollows_Followed 
            FOREIGN KEY ([FollowedId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT UQ_UserFollows_FollowerId_FollowedId UNIQUE ([FollowerId], [FollowedId]),
        CONSTRAINT CK_UserFollows_NoSelfFollow CHECK ([FollowerId] != [FollowedId])
    );
    
    -- Create indexes
    CREATE INDEX IX_UserFollows_FollowerId ON [dbo].[UserFollows]([FollowerId]);
    CREATE INDEX IX_UserFollows_FollowedId ON [dbo].[UserFollows]([FollowedId]);
    CREATE INDEX IX_UserFollows_FollowedId_IsActive ON [dbo].[UserFollows]([FollowedId], [IsActive]);
    
    PRINT '✅ Created UserFollows table';
END
ELSE
BEGIN
    PRINT '⚠️ UserFollows table already exists';
END
GO

-- ================================================================
-- VERIFICATION: Check all tables
-- ================================================================
PRINT '';
PRINT '========================================';
PRINT 'Verification Results:';
PRINT '========================================';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostReports]'))
    PRINT '✅ PostReports table EXISTS'
ELSE
    PRINT '❌ PostReports table MISSING';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailQueues]'))
    PRINT '✅ EmailQueues table EXISTS'
ELSE
    PRINT '❌ EmailQueues table MISSING';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationPreferences]'))
    PRINT '✅ NotificationPreferences table EXISTS'
ELSE
    PRINT '❌ NotificationPreferences table MISSING';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationSettings]'))
    PRINT '✅ UserNotificationSettings table EXISTS'
ELSE
    PRINT '❌ UserNotificationSettings table MISSING';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserFollows]'))
    PRINT '✅ UserFollows table EXISTS'
ELSE
    PRINT '❌ UserFollows table MISSING';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]'))
    PRINT '✅ Notifications table EXISTS'
ELSE
    PRINT '❌ Notifications table MISSING';

PRINT '';
PRINT '========================================';
PRINT '✅ Database setup complete!';
PRINT '========================================';
GO

