-- Diagnostic SQL Query to Check Post Content
-- Run this to verify if Content exists in database for PostId 117

SELECT 
    PostId,
    Title,
    PostType,
    LEN(Content) as ContentLength,
    CASE 
        WHEN Content IS NULL THEN 'NULL'
        WHEN LEN(Content) = 0 THEN 'EMPTY STRING'
        ELSE 'HAS CONTENT'
    END as ContentStatus,
    LEFT(Content, 500) as ContentPreview,
    -- Check if base64 image exists in content
    CASE 
        WHEN Content LIKE '%data:image%' THEN 'YES'
        ELSE 'NO'
    END as HasBase64Image,
    -- Count img tags
    (LEN(Content) - LEN(REPLACE(Content, '<img', ''))) / 4 as ImageTagCount,
    CreatedAt,
    UpdatedAt
FROM Posts
WHERE PostId = 117;

-- Also check if there are any other posts with base64 images
SELECT TOP 10
    PostId,
    Title,
    LEN(Content) as ContentLength,
    CASE 
        WHEN Content LIKE '%data:image%' THEN 'YES'
        ELSE 'NO'
    END as HasBase64Image
FROM Posts
WHERE Content IS NOT NULL 
  AND LEN(Content) > 0
ORDER BY CreatedAt DESC;

