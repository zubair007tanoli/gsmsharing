-- Add MediaUrl and MediaType columns to StorySlides table
-- Run this script if the columns don't exist

-- Check if columns exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StorySlides]') AND name = 'MediaUrl')
BEGIN
    ALTER TABLE [dbo].[StorySlides]
    ADD [MediaUrl] NVARCHAR(500) NULL;
    PRINT 'Added MediaUrl column to StorySlides table';
END
ELSE
BEGIN
    PRINT 'MediaUrl column already exists in StorySlides table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StorySlides]') AND name = 'MediaType')
BEGIN
    ALTER TABLE [dbo].[StorySlides]
    ADD [MediaType] NVARCHAR(50) NULL;
    PRINT 'Added MediaType column to StorySlides table';
END
ELSE
BEGIN
    PRINT 'MediaType column already exists in StorySlides table';
END

GO

