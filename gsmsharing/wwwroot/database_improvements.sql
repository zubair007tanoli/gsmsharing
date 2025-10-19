-- =============================================
-- Database Optimization Script for gsmsharing
-- Generated: 2025-10-19
-- Description: Comprehensive database improvements
-- =============================================

USE [gsmsharing]
GO

PRINT '========================================='
PRINT 'Starting Database Optimization'
PRINT 'Database: gsmsharing'
PRINT 'Started at: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 1: Critical Performance Indexes
-- =============================================

PRINT '>>> SCRIPT 1: Creating Performance Indexes...'
PRINT ''

-- 1. MobilePosts Performance Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobilePosts_UserId_CreationDate' AND object_id = OBJECT_ID('dbo.MobilePosts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_MobilePosts_UserId_CreationDate 
    ON dbo.MobilePosts(UserId, CreationDate DESC)
    INCLUDE (Title, views, likes, publish)
    PRINT '✓ Created IX_MobilePosts_UserId_CreationDate'
END
ELSE PRINT '  IX_MobilePosts_UserId_CreationDate already exists'

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobilePosts_Publish_CreationDate' AND object_id = OBJECT_ID('dbo.MobilePosts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_MobilePosts_Publish_CreationDate 
    ON dbo.MobilePosts(publish, CreationDate DESC)
    WHERE publish = 1
    INCLUDE (Title, views, likes, UserId)
    PRINT '✓ Created IX_MobilePosts_Publish_CreationDate'
END
ELSE PRINT '  IX_MobilePosts_Publish_CreationDate already exists'

-- 2. UsersFourm Performance Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsersFourm_Publish_CreationDate' AND object_id = OBJECT_ID('dbo.UsersFourm'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UsersFourm_Publish_CreationDate 
    ON dbo.UsersFourm(publish, CreationDate DESC)
    WHERE publish = 1
    INCLUDE (Title, views, likes, UserId, ParantId)
    PRINT '✓ Created IX_UsersFourm_Publish_CreationDate'
END
ELSE PRINT '  IX_UsersFourm_Publish_CreationDate already exists'

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UsersFourm_ParantId' AND object_id = OBJECT_ID('dbo.UsersFourm'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UsersFourm_ParantId 
    ON dbo.UsersFourm(ParantId, CreationDate DESC)
    INCLUDE (Title, views, likes)
    PRINT '✓ Created IX_UsersFourm_ParantId'
END
ELSE PRINT '  IX_UsersFourm_ParantId already exists'

-- 3. BlogComments Performance Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BlogComments_BlogId_CreationDate' AND object_id = OBJECT_ID('dbo.BlogComments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_BlogComments_BlogId_CreationDate 
    ON dbo.BlogComments(BlogId, CreationDate DESC)
    INCLUDE (UserId, Comment)
    PRINT '✓ Created IX_BlogComments_BlogId_CreationDate'
END
ELSE PRINT '  IX_BlogComments_BlogId_CreationDate already exists'

-- 4. GsmBlog Performance Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GsmBlog_Publish_PublishDate' AND object_id = OBJECT_ID('dbo.GsmBlog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GsmBlog_Publish_PublishDate 
    ON dbo.GsmBlog(Publish, PublishDate DESC)
    WHERE Publish = 1
    INCLUDE (BlogTitle, CategoryId, BlogViews, BlogLikes)
    PRINT '✓ Created IX_GsmBlog_Publish_PublishDate'
END
ELSE PRINT '  IX_GsmBlog_Publish_PublishDate already exists'

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GsmBlog_CategoryId' AND object_id = OBJECT_ID('dbo.GsmBlog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GsmBlog_CategoryId 
    ON dbo.GsmBlog(CategoryId, PublishDate DESC)
    WHERE Publish = 1
    PRINT '✓ Created IX_GsmBlog_CategoryId'
END
ELSE PRINT '  IX_GsmBlog_CategoryId already exists'

-- 5. MobileAds Performance Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MobileAds_Publish_CreationDate' AND object_id = OBJECT_ID('dbo.MobileAds'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_MobileAds_Publish_CreationDate 
    ON dbo.MobileAds(publish, CreationDate DESC)
    WHERE publish = 1
    INCLUDE (Title, price, views, likes)
    PRINT '✓ Created IX_MobileAds_Publish_CreationDate'
END
ELSE PRINT '  IX_MobileAds_Publish_CreationDate already exists'

-- 6. AspNetUsers Email Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_Email' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AspNetUsers_Email 
    ON dbo.AspNetUsers(Email)
    INCLUDE (UserName, EmailConfirmed)
    PRINT '✓ Created IX_AspNetUsers_Email'
END
ELSE PRINT '  IX_AspNetUsers_Email already exists'

-- 7. gsmsharing schema indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_blogposts_Publish_CreationDate' AND object_id = OBJECT_ID('gsmsharing.blogposts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_blogposts_Publish_CreationDate 
    ON gsmsharing.blogposts(publish, creationDate DESC)
    WHERE publish = 1
    INCLUDE (Title, views, likes, UserID)
    PRINT '✓ Created IX_blogposts_Publish_CreationDate'
END
ELSE PRINT '  IX_blogposts_Publish_CreationDate already exists'

PRINT ''
PRINT 'Script 1 completed: Performance indexes created'
PRINT ''

-- =============================================
-- SCRIPT 2: Data Integrity Constraints
-- =============================================

PRINT '>>> SCRIPT 2: Adding Data Integrity Constraints...'
PRINT ''

-- Check constraints for publish flags
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobilePosts_Publish')
BEGIN
    ALTER TABLE dbo.MobilePosts 
    ADD CONSTRAINT CK_MobilePosts_Publish CHECK (publish IN (0, 1))
    PRINT '✓ Added CK_MobilePosts_Publish'
END
ELSE PRINT '  CK_MobilePosts_Publish already exists'

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_UsersFourm_Publish')
BEGIN
    ALTER TABLE dbo.UsersFourm 
    ADD CONSTRAINT CK_UsersFourm_Publish CHECK (publish IN (0, 1))
    PRINT '✓ Added CK_UsersFourm_Publish'
END
ELSE PRINT '  CK_UsersFourm_Publish already exists'

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobileAds_Publish')
BEGIN
    ALTER TABLE dbo.MobileAds 
    ADD CONSTRAINT CK_MobileAds_Publish CHECK (publish IN (0, 1))
    PRINT '✓ Added CK_MobileAds_Publish'
END
ELSE PRINT '  CK_MobileAds_Publish already exists'

-- Check constraints for ratings/votes
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobilePosts_Views')
BEGIN
    ALTER TABLE dbo.MobilePosts 
    ADD CONSTRAINT CK_MobilePosts_Views CHECK (views >= 0)
    PRINT '✓ Added CK_MobilePosts_Views'
END
ELSE PRINT '  CK_MobilePosts_Views already exists'

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobilePosts_Likes')
BEGIN
    ALTER TABLE dbo.MobilePosts 
    ADD CONSTRAINT CK_MobilePosts_Likes CHECK (likes >= 0)
    PRINT '✓ Added CK_MobilePosts_Likes'
END
ELSE PRINT '  CK_MobilePosts_Likes already exists'

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobilePosts_Dislikes')
BEGIN
    ALTER TABLE dbo.MobilePosts 
    ADD CONSTRAINT CK_MobilePosts_Dislikes CHECK (dislikes >= 0)
    PRINT '✓ Added CK_MobilePosts_Dislikes'
END
ELSE PRINT '  CK_MobilePosts_Dislikes already exists'

-- Check constraints for prices
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_MobileAds_Price')
BEGIN
    ALTER TABLE dbo.MobileAds 
    ADD CONSTRAINT CK_MobileAds_Price CHECK (price >= 0)
    PRINT '✓ Added CK_MobileAds_Price'
END
ELSE PRINT '  CK_MobileAds_Price already exists'

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AmazonProducts_CurrentPrice')
BEGIN
    ALTER TABLE dbo.AmazonProducts 
    ADD CONSTRAINT CK_AmazonProducts_CurrentPrice CHECK (current_price >= 0)
    PRINT '✓ Added CK_AmazonProducts_CurrentPrice'
END
ELSE PRINT '  CK_AmazonProducts_CurrentPrice already exists'

-- Check constraints for star ratings
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_AmazonProducts_StarRating')
BEGIN
    ALTER TABLE dbo.AmazonProducts 
    ADD CONSTRAINT CK_AmazonProducts_StarRating CHECK (star_rating >= 0 AND star_rating <= 5)
    PRINT '✓ Added CK_AmazonProducts_StarRating'
END
ELSE PRINT '  CK_AmazonProducts_StarRating already exists'

PRINT ''
PRINT 'Script 2 completed: Data integrity constraints added'
PRINT ''

-- =============================================
-- SCRIPT 3: Full-Text Search Configuration
-- =============================================

PRINT '>>> SCRIPT 3: Setting up Full-Text Search...'
PRINT ''

IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
BEGIN
    -- Create full-text catalog
    IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ftCatalog_gsmsharing')
    BEGIN
        CREATE FULLTEXT CATALOG ftCatalog_gsmsharing AS DEFAULT
        PRINT '✓ Created full-text catalog: ftCatalog_gsmsharing'
    END
    ELSE PRINT '  ftCatalog_gsmsharing already exists'

    -- MobilePosts full-text index
    IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.MobilePosts'))
    BEGIN
        CREATE FULLTEXT INDEX ON dbo.MobilePosts(Title, Content)
        KEY INDEX PK__MobilePo__54379E302C94E4A7
        ON ftCatalog_gsmsharing
        WITH CHANGE_TRACKING AUTO
        PRINT '✓ Created full-text index on MobilePosts'
    END
    ELSE PRINT '  Full-text index on MobilePosts already exists'

    -- UsersFourm full-text index
    IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.UsersFourm'))
    BEGIN
        CREATE FULLTEXT INDEX ON dbo.UsersFourm(Title, Content)
        KEY INDEX PK__UsersFou__938B20B1D6BDE15F
        ON ftCatalog_gsmsharing
        WITH CHANGE_TRACKING AUTO
        PRINT '✓ Created full-text index on UsersFourm'
    END
    ELSE PRINT '  Full-text index on UsersFourm already exists'

    -- GsmBlog full-text index
    IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.GsmBlog'))
    BEGIN
        CREATE FULLTEXT INDEX ON dbo.GsmBlog(BlogTitle, BlogContent, BlogKeywords)
        KEY INDEX PK_GsmBlog
        ON ftCatalog_gsmsharing
        WITH CHANGE_TRACKING AUTO
        PRINT '✓ Created full-text index on GsmBlog'
    END
    ELSE PRINT '  Full-text index on GsmBlog already exists'

    -- gsmsharing.blogposts full-text index
    IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('gsmsharing.blogposts'))
    BEGIN
        CREATE FULLTEXT INDEX ON gsmsharing.blogposts(Title, Content)
        KEY INDEX PK_blogposts_Blog_ID
        ON ftCatalog_gsmsharing
        WITH CHANGE_TRACKING AUTO
        PRINT '✓ Created full-text index on gsmsharing.blogposts'
    END
    ELSE PRINT '  Full-text index on gsmsharing.blogposts already exists'

    PRINT ''
    PRINT 'Script 3 completed: Full-text search configured'
END
ELSE
BEGIN
    PRINT '⚠ Full-text search is not installed on this SQL Server instance'
    PRINT '  Skipping full-text index creation'
END

PRINT ''

-- =============================================
-- SCRIPT 4: Database Maintenance
-- =============================================

PRINT '>>> SCRIPT 4: Running Database Maintenance...'
PRINT ''

-- Update statistics
PRINT 'Updating statistics on key tables...'
UPDATE STATISTICS dbo.MobilePosts WITH FULLSCAN
PRINT '✓ Updated statistics: MobilePosts'

UPDATE STATISTICS dbo.UsersFourm WITH FULLSCAN
PRINT '✓ Updated statistics: UsersFourm'

UPDATE STATISTICS dbo.GsmBlog WITH FULLSCAN
PRINT '✓ Updated statistics: GsmBlog'

UPDATE STATISTICS dbo.AspNetUsers WITH FULLSCAN
PRINT '✓ Updated statistics: AspNetUsers'

UPDATE STATISTICS dbo.MobileAds WITH FULLSCAN
PRINT '✓ Updated statistics: MobileAds'

UPDATE STATISTICS gsmsharing.blogposts WITH FULLSCAN
PRINT '✓ Updated statistics: gsmsharing.blogposts'

PRINT ''
PRINT 'Script 4 completed: Database maintenance finished'
PRINT ''

-- =============================================
-- SCRIPT 5: Performance Monitoring Views
-- =============================================

PRINT '>>> SCRIPT 5: Creating Performance Monitoring Views...'
PRINT ''

-- Table sizes view
IF OBJECT_ID('dbo.vw_TableSizes', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TableSizes

EXEC('
CREATE VIEW dbo.vw_TableSizes
AS
SELECT 
    t.name AS TableName,
    s.name AS SchemaName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB,
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.name NOT LIKE ''dt%'' AND i.object_id > 255 AND i.index_id <= 1
GROUP BY t.name, s.name, p.rows
')
PRINT '✓ Created vw_TableSizes'

-- Popular content view
IF OBJECT_ID('dbo.vw_PopularContent', 'V') IS NOT NULL
    DROP VIEW dbo.vw_PopularContent

EXEC('
CREATE VIEW dbo.vw_PopularContent
AS
SELECT TOP 100
    ''MobilePosts'' AS ContentType,
    BlogId AS ContentId,
    Title,
    views AS Views,
    likes AS Likes,
    dislikes AS Dislikes,
    CreationDate,
    publish AS IsPublished
FROM dbo.MobilePosts
WHERE publish = 1
UNION ALL
SELECT TOP 100
    ''UsersFourm'' AS ContentType,
    UserFourmID AS ContentId,
    Title,
    views AS Views,
    likes AS Likes,
    dislikes AS Dislikes,
    CreationDate,
    publish AS IsPublished
FROM dbo.UsersFourm
WHERE publish = 1
UNION ALL
SELECT TOP 100
    ''GsmBlog'' AS ContentType,
    BlogId AS ContentId,
    BlogTitle AS Title,
    BlogViews AS Views,
    BlogLikes AS Likes,
    BlogDisLikes AS Dislikes,
    PublishDate AS CreationDate,
    CAST(Publish AS tinyint) AS IsPublished
FROM dbo.GsmBlog
WHERE Publish = 1
')
PRINT '✓ Created vw_PopularContent'

-- User activity summary
IF OBJECT_ID('dbo.vw_UserActivitySummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_UserActivitySummary

EXEC('
CREATE VIEW dbo.vw_UserActivitySummary
AS
SELECT 
    u.Id AS UserId,
    u.UserName,
    u.Email,
    COUNT(DISTINCT mp.BlogId) AS TotalPosts,
    COUNT(DISTINCT uf.UserFourmID) AS TotalForumPosts,
    COUNT(DISTINCT bc.Commentid) AS TotalComments,
    SUM(ISNULL(mp.views, 0)) AS TotalPostViews,
    SUM(ISNULL(mp.likes, 0)) AS TotalPostLikes
FROM dbo.AspNetUsers u
LEFT JOIN dbo.MobilePosts mp ON u.Id = mp.UserId
LEFT JOIN dbo.UsersFourm uf ON u.Id = uf.UserId
LEFT JOIN dbo.BlogComments bc ON u.Id = bc.UserId
GROUP BY u.Id, u.UserName, u.Email
')
PRINT '✓ Created vw_UserActivitySummary'

PRINT ''
PRINT 'Script 5 completed: Performance monitoring views created'
PRINT ''

-- =============================================
-- SCRIPT 6: Database Configuration
-- =============================================

PRINT '>>> SCRIPT 6: Optimizing Database Configuration...'
PRINT ''

-- Enable Query Store
ALTER DATABASE [gsmsharing] SET QUERY_STORE = ON
ALTER DATABASE [gsmsharing] SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 1000,
    QUERY_CAPTURE_MODE = AUTO,
    SIZE_BASED_CLEANUP_MODE = AUTO
)
PRINT '✓ Enabled Query Store'

-- Set database options
ALTER DATABASE [gsmsharing] SET AUTO_UPDATE_STATISTICS_ASYNC ON
PRINT '✓ Enabled AUTO_UPDATE_STATISTICS_ASYNC'

ALTER DATABASE [gsmsharing] SET PAGE_VERIFY CHECKSUM
PRINT '✓ Set PAGE_VERIFY to CHECKSUM'

PRINT ''
PRINT 'Script 6 completed: Database configuration optimized'
PRINT ''

-- =============================================
-- SUMMARY REPORT
-- =============================================

PRINT '========================================='
PRINT 'Database Optimization Completed!'
PRINT 'Finished at: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''
PRINT 'Summary of improvements:'
PRINT '  ✓ Performance indexes created'
PRINT '  ✓ Data integrity constraints added'
PRINT '  ✓ Full-text search configured'
PRINT '  ✓ Statistics updated'
PRINT '  ✓ Monitoring views created'
PRINT '  ✓ Database configuration optimized'
PRINT ''
PRINT 'Next steps:'
PRINT '  1. Run: SELECT * FROM dbo.vw_TableSizes'
PRINT '  2. Run: SELECT * FROM dbo.vw_PopularContent'
PRINT '  3. Monitor query performance in Query Store'
PRINT ''
PRINT '========================================='
GO

