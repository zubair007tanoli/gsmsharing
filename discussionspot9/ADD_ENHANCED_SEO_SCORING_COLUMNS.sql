-- Add Enhanced SEO Scoring Columns to SeoScores Table
-- Run this script to add the new scoring columns

USE [DiscussionspotADO]
GO

-- Check if columns exist before adding
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SeoScores' AND COLUMN_NAME = 'ImageSeoScore')
BEGIN
    ALTER TABLE [SeoScores]
    ADD [ImageSeoScore] DECIMAL(5,2) NOT NULL DEFAULT 0.00;
    PRINT 'Added ImageSeoScore column';
END
ELSE
BEGIN
    PRINT 'ImageSeoScore column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SeoScores' AND COLUMN_NAME = 'TechnicalSeoScore')
BEGIN
    ALTER TABLE [SeoScores]
    ADD [TechnicalSeoScore] DECIMAL(5,2) NOT NULL DEFAULT 0.00;
    PRINT 'Added TechnicalSeoScore column';
END
ELSE
BEGIN
    PRINT 'TechnicalSeoScore column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SeoScores' AND COLUMN_NAME = 'ContentStructureScore')
BEGIN
    ALTER TABLE [SeoScores]
    ADD [ContentStructureScore] DECIMAL(5,2) NOT NULL DEFAULT 0.00;
    PRINT 'Added ContentStructureScore column';
END
ELSE
BEGIN
    PRINT 'ContentStructureScore column already exists';
END
GO

PRINT 'Enhanced SEO Scoring columns added successfully!';
GO

