-- Fix for Visual Editor Error: Invalid column name 'MediaType' and 'MediaUrl'
-- Run this script against your database

USE DiscussionspotADO;
GO

-- Add MediaUrl column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StorySlides]') AND name = 'MediaUrl')
BEGIN
    ALTER TABLE [dbo].[StorySlides]
    ADD [MediaUrl] NVARCHAR(500) NULL;
    PRINT 'Added MediaUrl column to StorySlides table';
END
ELSE
BEGIN
    PRINT 'MediaUrl column already exists';
END
GO

-- Add MediaType column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StorySlides]') AND name = 'MediaType')
BEGIN
    ALTER TABLE [dbo].[StorySlides]
    ADD [MediaType] NVARCHAR(50) NULL;
    PRINT 'Added MediaType column to StorySlides table';
END
ELSE
BEGIN
    PRINT 'MediaType column already exists';
END
GO

PRINT 'Script completed successfully!';
GO

