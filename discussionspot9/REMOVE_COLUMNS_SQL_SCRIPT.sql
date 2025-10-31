-- SQL Script to Remove BackgroundAudioUrl and PostId Columns from Stories Table
-- Run this script ONLY if the columns exist in your database

-- Check if BackgroundAudioUrl column exists and remove it
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Stories' 
    AND COLUMN_NAME = 'BackgroundAudioUrl'
)
BEGIN
    ALTER TABLE Stories DROP COLUMN BackgroundAudioUrl;
    PRINT 'Removed BackgroundAudioUrl column from Stories table';
END
ELSE
BEGIN
    PRINT 'BackgroundAudioUrl column does not exist in Stories table';
END

-- Check if PostId index exists and remove it
IF EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Stories_PostId' 
    AND object_id = OBJECT_ID('Stories')
)
BEGIN
    DROP INDEX IX_Stories_PostId ON Stories;
    PRINT 'Removed IX_Stories_PostId index from Stories table';
END
ELSE
BEGIN
    PRINT 'IX_Stories_PostId index does not exist';
END

-- Check if PostId foreign key exists and remove it
IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_Stories_Posts_PostId'
    AND parent_object_id = OBJECT_ID('Stories')
)
BEGIN
    ALTER TABLE Stories DROP CONSTRAINT FK_Stories_Posts_PostId;
    PRINT 'Removed FK_Stories_Posts_PostId foreign key from Stories table';
END
ELSE
BEGIN
    PRINT 'FK_Stories_Posts_PostId foreign key does not exist';
END

-- Check if PostId column exists and remove it
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Stories' 
    AND COLUMN_NAME = 'PostId'
)
BEGIN
    ALTER TABLE Stories DROP COLUMN PostId;
    PRINT 'Removed PostId column from Stories table';
END
ELSE
BEGIN
    PRINT 'PostId column does not exist in Stories table';
END

PRINT 'Script completed successfully!';

