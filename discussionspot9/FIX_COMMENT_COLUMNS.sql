-- Fix for Comment Features Error: Invalid column name 'IsPinned' and 'EditedAt'
-- Run this script against your database

USE DiscussionspotADO;
GO

-- Add IsPinned column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND name = 'IsPinned')
BEGIN
    ALTER TABLE [dbo].[Comments]
    ADD [IsPinned] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsPinned column to Comments table';
END
ELSE
BEGIN
    PRINT 'IsPinned column already exists';
END
GO

-- Add EditedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND name = 'EditedAt')
BEGIN
    ALTER TABLE [dbo].[Comments]
    ADD [EditedAt] DATETIME NULL;
    PRINT 'Added EditedAt column to Comments table';
END
ELSE
BEGIN
    PRINT 'EditedAt column already exists';
END
GO

-- Verify IsEdited column exists (should already be there from your model)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND name = 'IsEdited')
BEGIN
    ALTER TABLE [dbo].[Comments]
    ADD [IsEdited] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsEdited column to Comments table';
END
ELSE
BEGIN
    PRINT 'IsEdited column already exists';
END
GO

-- Create index for pinned comments (performance optimization)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND name = 'IX_Comments_IsPinned_PostId_CreatedAt')
BEGIN
    CREATE INDEX IX_Comments_IsPinned_PostId_CreatedAt 
    ON [dbo].[Comments](PostId, IsPinned, CreatedAt DESC)
    WHERE IsPinned = 1;
    PRINT 'Created index IX_Comments_IsPinned_PostId_CreatedAt';
END
ELSE
BEGIN
    PRINT 'Index IX_Comments_IsPinned_PostId_CreatedAt already exists';
END
GO

PRINT '========================================';
PRINT 'Comment columns migration completed successfully!';
PRINT '========================================';
PRINT 'Next step: Restart your application';
GO

