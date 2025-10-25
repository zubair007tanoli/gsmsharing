-- Migration: Create PostReports table for post reporting system
-- Purpose: Allow users to report posts and notify admins
-- Date: 2025-10-25

USE DiscussionspotADO;
GO

-- Create PostReports table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostReports]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostReports] (
        [ReportId] INT PRIMARY KEY IDENTITY(1,1),
        [PostId] INT NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [Reason] NVARCHAR(200) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending',
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [ReviewedAt] DATETIME NULL,
        [ReviewedByUserId] NVARCHAR(450) NULL,
        [AdminNotes] NVARCHAR(MAX) NULL,
        
        -- Foreign Keys
        CONSTRAINT [FK_PostReports_Posts] 
            FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostReports_User] 
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]),
        CONSTRAINT [FK_PostReports_ReviewedBy] 
            FOREIGN KEY ([ReviewedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id])
    );
    
    PRINT 'Created PostReports table';
END
ELSE
BEGIN
    PRINT 'PostReports table already exists';
END
GO

-- Create indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PostReports]') AND name = 'IX_PostReports_Status_CreatedAt')
BEGIN
    CREATE INDEX IX_PostReports_Status_CreatedAt 
    ON [dbo].[PostReports]([Status], [CreatedAt] DESC);
    PRINT 'Created index IX_PostReports_Status_CreatedAt';
END
ELSE
BEGIN
    PRINT 'Index IX_PostReports_Status_CreatedAt already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PostReports]') AND name = 'IX_PostReports_PostId')
BEGIN
    CREATE INDEX IX_PostReports_PostId 
    ON [dbo].[PostReports]([PostId]);
    PRINT 'Created index IX_PostReports_PostId';
END
ELSE
BEGIN
    PRINT 'Index IX_PostReports_PostId already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PostReports]') AND name = 'IX_PostReports_UserId')
BEGIN
    CREATE INDEX IX_PostReports_UserId 
    ON [dbo].[PostReports]([UserId]);
    PRINT 'Created index IX_PostReports_UserId';
END
ELSE
BEGIN
    PRINT 'Index IX_PostReports_UserId already exists';
END
GO

PRINT '========================================';
PRINT 'PostReports table migration completed!';
PRINT '========================================';
GO

