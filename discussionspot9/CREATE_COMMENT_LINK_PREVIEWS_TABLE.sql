-- ============================================
-- Create CommentLinkPreviews Table
-- This table stores cached link preview metadata for URLs in comments
-- ============================================

USE [YourDatabaseName] -- CHANGE THIS TO YOUR ACTUAL DATABASE NAME
GO

-- Check if table exists, drop if it does (for clean install)
IF OBJECT_ID('dbo.CommentLinkPreviews', 'U') IS NOT NULL
BEGIN
    PRINT 'Dropping existing CommentLinkPreviews table...'
    DROP TABLE [dbo].[CommentLinkPreviews]
END
GO

-- Create the CommentLinkPreviews table
PRINT 'Creating CommentLinkPreviews table...'
CREATE TABLE [dbo].[CommentLinkPreviews] (
    [CommentLinkPreviewId] INT IDENTITY(1,1) NOT NULL,
    [CommentId] INT NOT NULL,
    [Url] NVARCHAR(2048) NOT NULL,
    [Title] NVARCHAR(500) NULL,
    [Description] NVARCHAR(1000) NULL,
    [Domain] NVARCHAR(255) NULL,
    [ThumbnailUrl] NVARCHAR(2048) NULL,
    [FaviconUrl] NVARCHAR(2048) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [LastFetchedAt] DATETIME2(7) NULL,
    [FetchSucceeded] BIT NOT NULL DEFAULT 0,
    
    -- Primary Key
    CONSTRAINT [PK_CommentLinkPreviews] PRIMARY KEY CLUSTERED ([CommentLinkPreviewId] ASC),
    
    -- Foreign Key to Comments table
    CONSTRAINT [FK_CommentLinkPreviews_Comments_CommentId] FOREIGN KEY ([CommentId])
        REFERENCES [dbo].[Comments] ([CommentId]) 
        ON DELETE CASCADE
)
GO

-- Create index on CommentId for faster lookups
PRINT 'Creating index on CommentId...'
CREATE NONCLUSTERED INDEX [IX_CommentLinkPreviews_CommentId]
    ON [dbo].[CommentLinkPreviews]([CommentId] ASC)
GO

-- Create index on Url for faster cache lookups
PRINT 'Creating index on Url...'
CREATE NONCLUSTERED INDEX [IX_CommentLinkPreviews_Url]
    ON [dbo].[CommentLinkPreviews]([Url] ASC)
GO

-- Create index on LastFetchedAt for cache expiry queries
PRINT 'Creating index on LastFetchedAt...'
CREATE NONCLUSTERED INDEX [IX_CommentLinkPreviews_LastFetchedAt]
    ON [dbo].[CommentLinkPreviews]([LastFetchedAt] ASC)
    WHERE [LastFetchedAt] IS NOT NULL
GO

PRINT 'CommentLinkPreviews table created successfully!'
GO

-- Verify the table was created
IF OBJECT_ID('dbo.CommentLinkPreviews', 'U') IS NOT NULL
BEGIN
    PRINT '✅ SUCCESS: CommentLinkPreviews table exists'
    
    -- Display table structure
    SELECT 
        COLUMN_NAME AS [Column],
        DATA_TYPE AS [Type],
        CHARACTER_MAXIMUM_LENGTH AS [MaxLength],
        IS_NULLABLE AS [Nullable]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CommentLinkPreviews'
    ORDER BY ORDINAL_POSITION
END
ELSE
BEGIN
    PRINT '❌ ERROR: Table creation failed'
END
GO

