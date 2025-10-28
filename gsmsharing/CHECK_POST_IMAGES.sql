-- Check recent posts and their FeaturedImage values
-- This script helps diagnose image upload issues

-- Check the most recent posts
SELECT TOP 10
    PostID,
    Title,
    Slug,
    FeaturedImage,
    CreatedAt,
    CASE 
        WHEN FeaturedImage IS NULL THEN '❌ NULL'
        WHEN FeaturedImage = '' THEN '❌ EMPTY STRING'
        ELSE '✅ HAS VALUE'
    END AS ImageStatus
FROM Posts
ORDER BY CreatedAt DESC;

-- Count posts with and without images
SELECT 
    COUNT(*) AS TotalPosts,
    SUM(CASE WHEN FeaturedImage IS NOT NULL AND FeaturedImage != '' THEN 1 ELSE 0 END) AS PostsWithImages,
    SUM(CASE WHEN FeaturedImage IS NULL OR FeaturedImage = '' THEN 1 ELSE 0 END) AS PostsWithoutImages
FROM Posts;

-- Check the specific post mentioned by the user
SELECT 
    PostID,
    Title,
    Slug,
    FeaturedImage,
    Content,
    UserId,
    CreatedAt,
    UpdatedAt
FROM Posts
WHERE Slug LIKE '%apple-liquid-glass-tint%'
   OR Title LIKE '%apple liquid glass tint%';

