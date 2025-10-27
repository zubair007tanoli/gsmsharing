-- =============================================
-- Diagnostic Script for Post ID 85
-- iOS 26.1 Update Post Investigation
-- =============================================

USE [gsmsharing]
GO

PRINT '========================================='
PRINT 'POST 85 DIAGNOSTIC REPORT'
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''

-- =============================================
-- CHECK 1: Post Data
-- =============================================

PRINT '>>> CHECK 1: Post Details'
PRINT ''

SELECT 
    PostId,
    Title,
    Slug,
    PostType,
    Content,
    Url,
    Status,
    CreatedAt,
    UpdatedAt,
    ViewCount,
    CommentCount,
    Score,
    UserId,
    CommunityId
FROM Posts
WHERE PostId = 85

PRINT ''

-- =============================================
-- CHECK 2: Media Records
-- =============================================

PRINT '>>> CHECK 2: Media Records for Post 85'
PRINT ''

SELECT 
    MediaId,
    Url,
    ThumbnailUrl,
    MediaType,
    ContentType,
    FileName,
    FileSize,
    Width,
    Height,
    Caption,
    AltText,
    UploadedAt,
    StorageProvider,
    StoragePath,
    IsProcessed,
    UserId,
    PostId
FROM Media
WHERE PostId = 85
ORDER BY MediaId

PRINT ''

-- Count check
DECLARE @MediaCount INT
SELECT @MediaCount = COUNT(*) FROM Media WHERE PostId = 85

IF @MediaCount = 0
    PRINT '⚠️ WARNING: No media records found for Post 85'
ELSE
    PRINT '✅ Found ' + CAST(@MediaCount AS VARCHAR) + ' media record(s)'

PRINT ''

-- =============================================
-- CHECK 3: File Validation
-- =============================================

PRINT '>>> CHECK 3: Media URL Analysis'
PRINT ''

SELECT 
    MediaId,
    Url,
    CASE 
        WHEN Url IS NULL THEN '❌ NULL'
        WHEN Url = '' THEN '❌ EMPTY'
        WHEN Url LIKE 'http%' THEN '✅ Absolute URL (external)'
        WHEN Url LIKE '/%' THEN '✅ Relative URL (starts with /)'
        WHEN Url LIKE 'upload%' THEN '⚠️ Missing leading slash'
        ELSE '❓ Unknown format'
    END as URLFormat,
    CASE 
        WHEN StorageProvider = 'local' THEN '💾 Local file (should exist on server)'
        WHEN StorageProvider = 'external' THEN '🔗 External URL (no file on server)'
        ELSE '❓ Unknown'
    END as StorageType,
    CASE 
        WHEN IsProcessed = 1 THEN '✅ Processed'
        ELSE '❌ Not Processed'
    END as ProcessingStatus
FROM Media
WHERE PostId = 85

PRINT ''

-- =============================================
-- CHECK 4: Expected File Paths
-- =============================================

PRINT '>>> CHECK 4: Expected File Locations'
PRINT ''

SELECT 
    MediaId,
    'Local file should exist at: ' + Url as ExpectedPath,
    'Access via browser: https://discussionspot.com' + Url as BrowserURL
FROM Media
WHERE PostId = 85 AND StorageProvider = 'local'

PRINT ''

-- =============================================
-- CHECK 5: Community & User Info
-- =============================================

PRINT '>>> CHECK 5: Related Data'
PRINT ''

-- Community
SELECT 
    c.CommunityId,
    c.Name as CommunityName,
    c.Slug as CommunitySlug
FROM Posts p
JOIN Communities c ON p.CommunityId = c.CommunityId
WHERE p.PostId = 85

-- User
SELECT 
    u.Id as UserId,
    u.UserName,
    u.Email
FROM Posts p
JOIN AspNetUsers u ON p.UserId = u.Id
WHERE p.PostId = 85

PRINT ''

-- =============================================
-- CHECK 6: Recent Similar Posts (for comparison)
-- =============================================

PRINT '>>> CHECK 6: Recent Posts with Media'
PRINT ''

SELECT TOP 5
    p.PostId,
    p.Title,
    p.PostType,
    COUNT(m.MediaId) as MediaCount,
    STRING_AGG(m.Url, '; ') as MediaUrls,
    p.CreatedAt
FROM Posts p
LEFT JOIN Media m ON p.PostId = m.PostId
WHERE p.PostId != 85
  AND p.CreatedAt > DATEADD(day, -7, GETDATE())
  AND EXISTS (SELECT 1 FROM Media WHERE PostId = p.PostId)
GROUP BY p.PostId, p.Title, p.PostType, p.CreatedAt
ORDER BY p.CreatedAt DESC

PRINT ''

-- =============================================
-- SUMMARY & RECOMMENDATIONS
-- =============================================

PRINT '========================================='
PRINT 'DIAGNOSTIC SUMMARY'
PRINT '========================================='
PRINT ''

DECLARE @HasMedia BIT = 0
DECLARE @MediaUrl VARCHAR(500) = NULL
DECLARE @StorageType VARCHAR(50) = NULL

SELECT 
    @HasMedia = 1,
    @MediaUrl = Url,
    @StorageType = StorageProvider
FROM Media
WHERE PostId = 85 AND MediaId = 37

IF @HasMedia = 1
BEGIN
    PRINT '✅ Media record EXISTS in database'
    PRINT '   MediaId: 37'
    PRINT '   URL: ' + ISNULL(@MediaUrl, 'NULL')
    PRINT '   Storage: ' + ISNULL(@StorageType, 'NULL')
    PRINT ''
    
    IF @StorageType = 'local'
    BEGIN
        PRINT '📁 LOCAL FILE - Action Items:'
        PRINT '   1. Check if file exists on server:'
        PRINT '      Path: /path/to/site/wwwroot' + @MediaUrl
        PRINT '   2. Check file permissions'
        PRINT '   3. Test browser access:'
        PRINT '      https://discussionspot.com' + @MediaUrl
        PRINT ''
    END
    ELSE IF @StorageType = 'external'
    BEGIN
        PRINT '🔗 EXTERNAL URL - Action Items:'
        PRINT '   1. Test URL in browser:'
        PRINT '      ' + @MediaUrl
        PRINT '   2. Check if URL is accessible'
        PRINT '   3. Verify no CORS issues'
        PRINT ''
    END
    
    PRINT '🔍 NEXT STEPS:'
    PRINT '   1. Access post page: https://localhost:7025/r/gsmsharing/posts/ios-261-update-everything-you-need-to-know-2025'
    PRINT '   2. Open browser DevTools (F12)'
    PRINT '   3. Check Console for errors'
    PRINT '   4. Check Network tab for failed image requests'
    PRINT '   5. Check if Model.Post.Media is populated in view (add debug output)'
END
ELSE
BEGIN
    PRINT '❌ No media record found for Post 85'
    PRINT ''
    PRINT 'ACTIONS:'
    PRINT '   1. Create media record manually:'
    PRINT '      INSERT INTO Media (PostId, Url, MediaType, UploadedAt, StorageProvider, IsProcessed)'
    PRINT '      VALUES (85, ''/uploads/posts/images/your-image.jpg'', ''image'', GETUTCDATE(), ''local'', 1)'
    PRINT '   2. Or re-upload image through edit post function'
END

PRINT ''
PRINT '========================================='
PRINT 'END OF REPORT'
PRINT '========================================='
GO

