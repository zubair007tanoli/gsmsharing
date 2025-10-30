-- ================================================================
-- Add Enhanced Notification Columns
-- Run this script manually if migrations are problematic
-- ================================================================

-- Add Actor columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ActorUserId')
BEGIN
    ALTER TABLE [Notifications] ADD [ActorUserId] nvarchar(450) NULL;
    PRINT 'Added ActorUserId column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ActorDisplayName')
BEGIN
    ALTER TABLE [Notifications] ADD [ActorDisplayName] nvarchar(100) NULL;
    PRINT 'Added ActorDisplayName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ActorAvatarUrl')
BEGIN
    ALTER TABLE [Notifications] ADD [ActorAvatarUrl] nvarchar(2048) NULL;
    PRINT 'Added ActorAvatarUrl column';
END

-- Add URL column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'Url')
BEGIN
    ALTER TABLE [Notifications] ADD [Url] nvarchar(2048) NULL;
    PRINT 'Added Url column';
END

-- Add Email tracking columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'EmailSent')
BEGIN
    ALTER TABLE [Notifications] ADD [EmailSent] bit NOT NULL DEFAULT 0;
    PRINT 'Added EmailSent column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'EmailSentAt')
BEGIN
    ALTER TABLE [Notifications] ADD [EmailSentAt] datetime2 NULL;
    PRINT 'Added EmailSentAt column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ReadAt')
BEGIN
    ALTER TABLE [Notifications] ADD [ReadAt] datetime2 NULL;
    PRINT 'Added ReadAt column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'GroupId')
BEGIN
    ALTER TABLE [Notifications] ADD [GroupId] nvarchar(100) NULL;
    PRINT 'Added GroupId column';
END

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_ActorUserId' AND object_id = OBJECT_ID('Notifications'))
BEGIN
    CREATE INDEX [IX_Notifications_ActorUserId] ON [Notifications] ([ActorUserId]);
    PRINT 'Created index on ActorUserId';
END

-- Create foreign key for Actor
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_AspNetUsers_ActorUserId')
BEGIN
    ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_AspNetUsers_ActorUserId] 
        FOREIGN KEY ([ActorUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
    PRINT 'Created foreign key for ActorUserId';
END

PRINT 'Notification columns enhancement complete!';


