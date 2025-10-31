-- Fix AMP Stories Database Schema
-- This script ensures the database schema matches the C# models

-- 1. Remove any columns that don't exist in the models (if they exist)
-- Note: Only drop if they exist to avoid errors
IF COL_LENGTH('dbo.StorySlides', 'BackgroundAudioUrl') IS NOT NULL
BEGIN
    ALTER TABLE dbo.StorySlides DROP COLUMN BackgroundAudioUrl;
    PRINT 'Dropped BackgroundAudioUrl column from StorySlides';
END

IF COL_LENGTH('dbo.Stories', 'PostId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Stories DROP COLUMN PostId;
    PRINT 'Dropped PostId column from Stories';
END

-- 2. Ensure MediaUrl and MediaType columns exist (from previous fix)
IF COL_LENGTH('dbo.StorySlides', 'MediaUrl') IS NULL
BEGIN
    ALTER TABLE dbo.StorySlides ADD MediaUrl NVARCHAR(500) NULL;
    PRINT 'Added MediaUrl column to StorySlides';
END

IF COL_LENGTH('dbo.StorySlides', 'MediaType') IS NULL
BEGIN
    ALTER TABLE dbo.StorySlides ADD MediaType NVARCHAR(50) NULL;
    PRINT 'Added MediaType column to StorySlides';
END

-- 3. Add indexes for performance optimization
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StorySlides_StoryId_OrderIndex' AND object_id = OBJECT_ID('dbo.StorySlides'))
BEGIN
    CREATE INDEX IX_StorySlides_StoryId_OrderIndex ON dbo.StorySlides(StoryId, OrderIndex);
    PRINT 'Created index IX_StorySlides_StoryId_OrderIndex';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stories_Status_PublishedAt' AND object_id = OBJECT_ID('dbo.Stories'))
BEGIN
    CREATE INDEX IX_Stories_Status_PublishedAt ON dbo.Stories(Status, PublishedAt);
    PRINT 'Created index IX_Stories_Status_PublishedAt';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stories_Slug' AND object_id = OBJECT_ID('dbo.Stories'))
BEGIN
    CREATE INDEX IX_Stories_Slug ON dbo.Stories(Slug);
    PRINT 'Created index IX_Stories_Slug';
END

-- 4. Update any NULL values to defaults where needed
UPDATE dbo.StorySlides 
SET Duration = 5000 
WHERE Duration IS NULL OR Duration = 0;

UPDATE dbo.StorySlides 
SET SlideType = 'media' 
WHERE SlideType IS NULL OR SlideType = '';

UPDATE dbo.StorySlides 
SET Alignment = 'center' 
WHERE Alignment IS NULL OR Alignment = '';

UPDATE dbo.Stories 
SET Status = 'draft' 
WHERE Status IS NULL OR Status = '';

UPDATE dbo.Stories 
SET IsAmpEnabled = 1 
WHERE IsAmpEnabled IS NULL;

-- 5. Ensure proper data types match model definitions
-- MediaUrl should be NVARCHAR(500)
IF COL_LENGTH('dbo.StorySlides', 'MediaUrl') < 500
BEGIN
    ALTER TABLE dbo.StorySlides ALTER COLUMN MediaUrl NVARCHAR(500) NULL;
    PRINT 'Updated MediaUrl column size to 500';
END

-- MediaType should be NVARCHAR(50)
IF COL_LENGTH('dbo.StorySlides', 'MediaType') < 50
BEGIN
    ALTER TABLE dbo.StorySlides ALTER COLUMN MediaType NVARCHAR(50) NULL;
    PRINT 'Updated MediaType column size to 50';
END

PRINT 'Database schema fix completed successfully!';

