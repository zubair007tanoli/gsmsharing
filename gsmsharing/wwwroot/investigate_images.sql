-- =============================================
-- Image Upload Investigation Script
-- For gsmsharing/DiscussionSpot Database
-- =============================================

USE [gsmsharing]
GO

PRINT '========================================='
PRINT 'IMAGE UPLOAD INVESTIGATION'
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 1: Check Recent Posts with Images
-- =============================================

PRINT '>>> SCRIPT 1: Checking Recent Posts...'
PRINT ''

SELECT TOP 10
    PostID,
    Title,
    Slug,
    FeaturedImage,
    CASE 
        WHEN FeaturedImage IS NULL THEN 'NULL'
        WHEN FeaturedImage = '' THEN 'EMPTY STRING'
        WHEN LEN(FeaturedImage) > 0 THEN 'HAS VALUE (' + CAST(LEN(FeaturedImage) AS VARCHAR) + ' chars)'
    END as ImageStatus,
    CreatedAt,
    UpdatedAt,
    CommunityID,
    UserId
FROM Posts
ORDER BY CreatedAt DESC

PRINT ''
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 2: Check Specific iOS Post
-- =============================================

PRINT '>>> SCRIPT 2: Checking iOS 26.1 Update Post...'
PRINT ''

SELECT 
    PostID,
    Title,
    Slug,
    FeaturedImage as ImageURL,
    LEN(FeaturedImage) as URLLength,
    Content,
    CreatedAt,
    UpdatedAt,
    CommunityID,
    UserId,
    ViewCount,
    PostStatus
FROM Posts
WHERE Slug LIKE '%ios%26%' OR Slug LIKE '%ios-261%' OR Title LIKE '%iOS 26.1%'

PRINT ''
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 3: Statistics on Image Usage
-- =============================================

PRINT '>>> SCRIPT 3: Image Usage Statistics...'
PRINT ''

SELECT 
    'Total Posts' as Metric,
    COUNT(*) as Count
FROM Posts

UNION ALL

SELECT 
    'Posts with Images',
    COUNT(*)
FROM Posts
WHERE FeaturedImage IS NOT NULL AND FeaturedImage != ''

UNION ALL

SELECT 
    'Posts without Images',
    COUNT(*)
FROM Posts
WHERE FeaturedImage IS NULL OR FeaturedImage = ''

UNION ALL

SELECT 
    'Posts Created Today',
    COUNT(*)
FROM Posts
WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)

PRINT ''
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 4: Check for Media/File Tables
-- =============================================

PRINT '>>> SCRIPT 4: Checking for Related Tables...'
PRINT ''

-- Check if any media-related tables exist
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%media%' 
   OR TABLE_NAME LIKE '%file%'
   OR TABLE_NAME LIKE '%image%'
   OR TABLE_NAME LIKE '%upload%'
ORDER BY TABLE_NAME

PRINT ''
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 5: Check Image URL Patterns
-- =============================================

PRINT '>>> SCRIPT 5: Analyzing Image URL Patterns...'
PRINT ''

SELECT 
    CASE 
        WHEN FeaturedImage LIKE 'http%' THEN 'Absolute URL (http/https)'
        WHEN FeaturedImage LIKE '/%' THEN 'Relative URL (starts with /)'
        WHEN FeaturedImage LIKE 'upload%' OR FeaturedImage LIKE 'posts%' THEN 'Relative path (no leading /)'
        WHEN FeaturedImage IS NULL THEN 'NULL'
        WHEN FeaturedImage = '' THEN 'Empty'
        ELSE 'Unknown format'
    END as URLPattern,
    COUNT(*) as Count,
    CASE 
        WHEN COUNT(*) > 0 THEN 
            (SELECT TOP 1 FeaturedImage 
             FROM Posts p2 
             WHERE CASE 
                    WHEN p2.FeaturedImage LIKE 'http%' THEN 'Absolute URL (http/https)'
                    WHEN p2.FeaturedImage LIKE '/%' THEN 'Relative URL (starts with /)'
                    WHEN p2.FeaturedImage LIKE 'upload%' OR p2.FeaturedImage LIKE 'posts%' THEN 'Relative path (no leading /)'
                    WHEN p2.FeaturedImage IS NULL THEN 'NULL'
                    WHEN p2.FeaturedImage = '' THEN 'Empty'
                    ELSE 'Unknown format'
                   END = CASE 
                           WHEN Posts.FeaturedImage LIKE 'http%' THEN 'Absolute URL (http/https)'
                           WHEN Posts.FeaturedImage LIKE '/%' THEN 'Relative URL (starts with /)'
                           WHEN Posts.FeaturedImage LIKE 'upload%' OR Posts.FeaturedImage LIKE 'posts%' THEN 'Relative path (no leading /)'
                           WHEN Posts.FeaturedImage IS NULL THEN 'NULL'
                           WHEN Posts.FeaturedImage = '' THEN 'Empty'
                           ELSE 'Unknown format'
                         END
             AND p2.FeaturedImage IS NOT NULL
             AND p2.FeaturedImage != '')
        ELSE NULL
    END as ExampleURL
FROM Posts
GROUP BY 
    CASE 
        WHEN FeaturedImage LIKE 'http%' THEN 'Absolute URL (http/https)'
        WHEN FeaturedImage LIKE '/%' THEN 'Relative URL (starts with /)'
        WHEN FeaturedImage LIKE 'upload%' OR FeaturedImage LIKE 'posts%' THEN 'Relative path (no leading /)'
        WHEN FeaturedImage IS NULL THEN 'NULL'
        WHEN FeaturedImage = '' THEN 'Empty'
        ELSE 'Unknown format'
    END
ORDER BY Count DESC

PRINT ''
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 6: Check Community Structure
-- =============================================

PRINT '>>> SCRIPT 6: Checking Communities...'
PRINT ''

SELECT 
    c.CommunityID,
    c.Name as CommunityName,
    c.Slug as CommunitySlug,
    COUNT(p.PostID) as TotalPosts,
    SUM(CASE WHEN p.FeaturedImage IS NOT NULL AND p.FeaturedImage != '' THEN 1 ELSE 0 END) as PostsWithImages
FROM Communities c
LEFT JOIN Posts p ON c.CommunityID = p.CommunityID
GROUP BY c.CommunityID, c.Name, c.Slug
ORDER BY TotalPosts DESC

PRINT ''
PRINT '========================================='
PRINT ''

-- =============================================
-- SCRIPT 7: Sample FIX Script (if needed)
-- =============================================

PRINT '>>> SCRIPT 7: Sample Fix Script (REVIEW BEFORE RUNNING)...'
PRINT ''
PRINT '-- IMPORTANT: Review and modify before running!'
PRINT '-- This is a TEMPLATE, not ready to execute'
PRINT ''
PRINT '-- Fix: Add missing leading slash to image URLs'
PRINT '-- UPDATE Posts'
PRINT '-- SET FeaturedImage = ''/'' + FeaturedImage'
PRINT '-- WHERE FeaturedImage NOT LIKE ''/%'' '
PRINT '--   AND FeaturedImage NOT LIKE ''http%'''
PRINT '--   AND FeaturedImage IS NOT NULL'
PRINT '--   AND FeaturedImage != '''''
PRINT ''
PRINT '-- Fix: Update specific post with image URL'
PRINT '-- UPDATE Posts'
PRINT '-- SET FeaturedImage = ''/uploads/posts/featured/your-image-name.jpg'''
PRINT '-- WHERE Slug = ''ios-261-update-everything-you-need-to-know-2025'''
PRINT ''

PRINT '========================================='
PRINT 'INVESTIGATION COMPLETE'
PRINT 'Finished: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''
PRINT 'ACTION ITEMS:'
PRINT '  1. Review the posts with NULL/empty FeaturedImage'
PRINT '  2. Check if image files physically exist on server'
PRINT '  3. Verify upload directory permissions'
PRINT '  4. Check application logs for upload errors'
PRINT '  5. Test new upload with detailed logging enabled'
PRINT ''
GO

