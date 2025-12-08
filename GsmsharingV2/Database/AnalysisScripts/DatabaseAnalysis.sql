-- =============================================
-- GSMSharing V2 - Database Analysis Script
-- Database: gsmsharingv3
-- Purpose: Complete database schema analysis
-- =============================================

USE gsmsharingv3;
GO

PRINT '========================================';
PRINT 'GSMSharing V2 - Database Analysis';
PRINT 'Database: ' + DB_NAME();
PRINT 'Analysis Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. DATABASE INFORMATION
-- =============================================
PRINT '1. DATABASE INFORMATION';
PRINT '----------------------------------------';
SELECT 
    @@VERSION AS SQLServerVersion,
    DB_NAME() AS DatabaseName,
    DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS Collation,
    DATABASEPROPERTYEX(DB_NAME(), 'Recovery') AS RecoveryModel;
PRINT '';

-- =============================================
-- 2. LIST ALL TABLES
-- =============================================
PRINT '2. ALL TABLES IN DATABASE';
PRINT '----------------------------------------';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE,
    (SELECT COUNT(*) 
     FROM INFORMATION_SCHEMA.COLUMNS c 
     WHERE c.TABLE_NAME = t.TABLE_NAME 
     AND c.TABLE_SCHEMA = t.TABLE_SCHEMA) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;
PRINT 'Total Tables: ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS VARCHAR);
PRINT '';

-- =============================================
-- 3. TABLE DETAILS (Key Tables)
-- =============================================
PRINT '3. KEY TABLES DETAILS';
PRINT '----------------------------------------';

-- Posts Table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts')
BEGIN
    PRINT 'Posts Table:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH AS MAX_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Posts'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END

-- Communities Table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Communities')
BEGIN
    PRINT 'Communities Table:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH AS MAX_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Communities'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END

-- MobileAds Table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileAds')
BEGIN
    PRINT 'MobileAds Table:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH AS MAX_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'MobileAds'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END

-- UsersFourm Table (Forum)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsersFourm')
BEGIN
    PRINT 'UsersFourm (Forum) Table:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH AS MAX_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UsersFourm'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END

-- MobileSpecs Table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileSpecs')
BEGIN
    PRINT 'MobileSpecs Table:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH AS MAX_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'MobileSpecs'
    ORDER BY ORDINAL_POSITION;
    PRINT '';
END

-- =============================================
-- 4. FOREIGN KEY RELATIONSHIPS
-- =============================================
PRINT '4. FOREIGN KEY RELATIONSHIPS';
PRINT '----------------------------------------';
SELECT 
    OBJECT_NAME(f.parent_object_id) AS ParentTable,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ParentColumn,
    OBJECT_NAME(f.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn,
    f.name AS ForeignKeyName,
    f.delete_referential_action_desc AS DeleteAction,
    f.update_referential_action_desc AS UpdateAction
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc 
    ON f.object_id = fc.constraint_object_id
ORDER BY ParentTable, ReferencedTable;
PRINT '';

-- =============================================
-- 5. INDEXES ANALYSIS
-- =============================================
PRINT '5. INDEXES ANALYSIS';
PRINT '----------------------------------------';
SELECT 
    OBJECT_NAME(OBJECT_ID) AS TableName,
    name AS IndexName,
    type_desc AS IndexType,
    is_unique,
    is_primary_key,
    is_unique_constraint,
    fill_factor
FROM sys.indexes
WHERE OBJECT_ID IN (
    SELECT OBJECT_ID 
    FROM sys.tables 
    WHERE name IN ('Posts', 'Communities', 'Comments', 'MobileAds', 'UsersFourm', 'MobileSpecs')
)
AND type_desc != 'HEAP'
ORDER BY TableName, IndexName;
PRINT '';

-- =============================================
-- 6. MISSING INDEXES (Recommendations)
-- =============================================
PRINT '6. MISSING INDEX RECOMMENDATIONS';
PRINT '----------------------------------------';
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    equality_columns,
    inequality_columns,
    included_columns,
    avg_user_impact,
    user_seeks,
    user_scans
FROM sys.dm_db_missing_index_details
WHERE database_id = DB_ID()
ORDER BY avg_user_impact DESC;
PRINT '';

-- =============================================
-- 7. TABLE SIZES
-- =============================================
PRINT '7. TABLE SIZES';
PRINT '----------------------------------------';
SELECT 
    t.NAME AS TableName,
    s.Name AS SchemaName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB,
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.NAME NOT LIKE 'dt%' 
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255
GROUP BY t.Name, s.Name, p.Rows
ORDER BY TotalSpaceKB DESC;
PRINT '';

-- =============================================
-- 8. CHECK CONSTRAINTS
-- =============================================
PRINT '8. CHECK CONSTRAINTS';
PRINT '----------------------------------------';
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    definition
FROM sys.check_constraints
ORDER BY TableName;
PRINT '';

-- =============================================
-- 9. DEFAULT CONSTRAINTS
-- =============================================
PRINT '9. DEFAULT CONSTRAINTS';
PRINT '----------------------------------------';
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    COL_NAME(parent_object_id, parent_column_id) AS ColumnName,
    name AS ConstraintName,
    definition
FROM sys.default_constraints
ORDER BY TableName, ColumnName;
PRINT '';

-- =============================================
-- 10. STORED PROCEDURES
-- =============================================
PRINT '10. STORED PROCEDURES';
PRINT '----------------------------------------';
SELECT 
    ROUTINE_SCHEMA,
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME;
PRINT '';

-- =============================================
-- 11. VIEWS
-- =============================================
PRINT '11. VIEWS';
PRINT '----------------------------------------';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    VIEW_DEFINITION
FROM INFORMATION_SCHEMA.VIEWS
ORDER BY TABLE_SCHEMA, TABLE_NAME;
PRINT '';

-- =============================================
-- 12. DATA COUNTS (Sample Data)
-- =============================================
PRINT '12. DATA COUNTS';
PRINT '----------------------------------------';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts')
    PRINT 'Posts: ' + CAST((SELECT COUNT(*) FROM Posts) AS VARCHAR);

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Communities')
    PRINT 'Communities: ' + CAST((SELECT COUNT(*) FROM Communities) AS VARCHAR);

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments')
    PRINT 'Comments: ' + CAST((SELECT COUNT(*) FROM Comments) AS VARCHAR);

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileAds')
    PRINT 'MobileAds: ' + CAST((SELECT COUNT(*) FROM MobileAds) AS VARCHAR);

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsersFourm')
    PRINT 'UsersFourm (Forum): ' + CAST((SELECT COUNT(*) FROM UsersFourm) AS VARCHAR);

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MobileSpecs')
    PRINT 'MobileSpecs: ' + CAST((SELECT COUNT(*) FROM MobileSpecs) AS VARCHAR);

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
    PRINT 'Users: ' + CAST((SELECT COUNT(*) FROM AspNetUsers) AS VARCHAR);

PRINT '';

-- =============================================
-- 13. INDEX FRAGMENTATION
-- =============================================
PRINT '13. INDEX FRAGMENTATION ANALYSIS';
PRINT '----------------------------------------';
SELECT 
    OBJECT_NAME(OBJECT_ID) AS TableName,
    name AS IndexName,
    avg_fragmentation_in_percent,
    page_count,
    avg_page_space_used_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED')
WHERE avg_fragmentation_in_percent > 10
ORDER BY avg_fragmentation_in_percent DESC;
PRINT '';

-- =============================================
-- ANALYSIS COMPLETE
-- =============================================
PRINT '========================================';
PRINT 'Database Analysis Complete!';
PRINT '========================================';

