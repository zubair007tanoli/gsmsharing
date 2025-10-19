-- =============================================
-- Database Improvements Verification Script
-- =============================================

USE [gsmsharing]
GO

PRINT '========================================'
PRINT 'Database Improvements Verification Report'
PRINT 'Generated: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================'
PRINT ''

-- Check 1: Performance Indexes
PRINT '1. PERFORMANCE INDEXES'
PRINT '----------------------------------------'
SELECT 
    OBJECT_SCHEMA_NAME(object_id) + '.' + OBJECT_NAME(object_id) AS TableName,
    name AS IndexName,
    type_desc AS IndexType
FROM sys.indexes 
WHERE (name LIKE 'IX_MobilePosts%' 
    OR name LIKE 'IX_UsersFourm%' 
    OR name LIKE 'IX_GsmBlog%'
    OR name LIKE 'IX_MobileAds%'
    OR name LIKE 'IX_BlogComments%'
    OR name LIKE 'IX_AspNetUsers_Email%'
    OR name LIKE 'IX_blogposts%')
ORDER BY TableName, name
PRINT ''

-- Check 2: Check Constraints
PRINT '2. DATA INTEGRITY CONSTRAINTS'
PRINT '----------------------------------------'
SELECT 
    OBJECT_SCHEMA_NAME(parent_object_id) + '.' + OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName
FROM sys.check_constraints 
WHERE name LIKE 'CK_%' AND create_date > DATEADD(day, -1, GETDATE())
ORDER BY TableName, name
PRINT ''

-- Check 3: Full-Text Catalogs and Indexes
PRINT '3. FULL-TEXT SEARCH'
PRINT '----------------------------------------'
PRINT 'Full-Text Catalogs:'
SELECT name AS CatalogName, create_date 
FROM sys.fulltext_catalogs
WHERE name = 'ftCatalog_gsmsharing'
PRINT ''

PRINT 'Full-Text Indexes:'
SELECT 
    OBJECT_SCHEMA_NAME(object_id) + '.' + OBJECT_NAME(object_id) AS TableName,
    'Enabled' AS Status
FROM sys.fulltext_indexes
WHERE object_id IN (
    OBJECT_ID('dbo.MobilePosts'),
    OBJECT_ID('dbo.UsersFourm'),
    OBJECT_ID('dbo.GsmBlog'),
    OBJECT_ID('gsmsharing.blogposts')
)
PRINT ''

-- Check 4: Monitoring Views
PRINT '4. PERFORMANCE MONITORING VIEWS'
PRINT '----------------------------------------'
SELECT 
    name AS ViewName,
    create_date AS CreatedDate
FROM sys.objects 
WHERE type = 'V' AND name LIKE 'vw_%'
ORDER BY name
PRINT ''

-- Check 5: Database Configuration
PRINT '5. DATABASE CONFIGURATION'
PRINT '----------------------------------------'
SELECT 
    'Query Store' AS Feature,
    CASE WHEN is_query_store_on = 1 THEN 'Enabled' ELSE 'Disabled' END AS Status
FROM sys.databases
WHERE name = 'gsmsharing'
UNION ALL
SELECT 
    'Auto Update Stats Async' AS Feature,
    CASE WHEN is_auto_update_stats_async_on = 1 THEN 'Enabled' ELSE 'Disabled' END
FROM sys.databases
WHERE name = 'gsmsharing'
UNION ALL
SELECT 
    'Read Committed Snapshot' AS Feature,
    CASE WHEN is_read_committed_snapshot_on = 1 THEN 'Enabled' ELSE 'Disabled' END
FROM sys.databases
WHERE name = 'gsmsharing'
PRINT ''

-- Check 6: Table Statistics (Top 10 Tables)
PRINT '6. TABLE SIZES (Top 10)'
PRINT '----------------------------------------'
SELECT TOP 10 * FROM dbo.vw_TableSizes 
ORDER BY TotalSpaceKB DESC
PRINT ''

-- Summary Count
PRINT '========================================'
PRINT 'SUMMARY'
PRINT '========================================'
SELECT 
    (SELECT COUNT(*) FROM sys.indexes WHERE name LIKE 'IX_%' AND object_id IN (
        OBJECT_ID('dbo.MobilePosts'), OBJECT_ID('dbo.UsersFourm'), OBJECT_ID('dbo.GsmBlog'),
        OBJECT_ID('dbo.MobileAds'), OBJECT_ID('dbo.BlogComments'), OBJECT_ID('dbo.AspNetUsers'),
        OBJECT_ID('gsmsharing.blogposts')
    )) AS TotalNewIndexes,
    (SELECT COUNT(*) FROM sys.check_constraints WHERE name LIKE 'CK_%' AND create_date > DATEADD(day, -1, GETDATE())) AS TotalNewConstraints,
    (SELECT COUNT(*) FROM sys.fulltext_indexes WHERE object_id IN (
        OBJECT_ID('dbo.MobilePosts'), OBJECT_ID('dbo.UsersFourm'), 
        OBJECT_ID('dbo.GsmBlog'), OBJECT_ID('gsmsharing.blogposts')
    )) AS TotalFullTextIndexes,
    (SELECT COUNT(*) FROM sys.objects WHERE type = 'V' AND name LIKE 'vw_%') AS TotalMonitoringViews
PRINT ''
PRINT '========================================'
PRINT 'Verification Complete!'
PRINT '========================================'
GO

