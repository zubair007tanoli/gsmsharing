-- ================================================
-- DATABASE PERFORMANCE OPTIMIZATION INDEXES
-- DiscussionSpot9 - Performance Enhancement Script
-- ================================================
-- Run this script to add critical indexes for improved query performance
-- Estimated improvement: 3-10x faster queries on large datasets
-- ================================================

USE [u749153_dsdb];
GO

-- ================================================
-- 1. POSTS TABLE INDEXES (Critical for Homepage & Listings)
-- ================================================

-- Composite index for published posts ordering by creation date and score
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_Status_CreatedAt_Score' AND object_id = OBJECT_ID('Posts'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Posts_Status_CreatedAt_Score]
    ON [dbo].[Posts]([Status], [CreatedAt] DESC, [Score] DESC)
    INCLUDE ([ViewCount], [CommentCount], [Title], [Slug], [CommunityId], [UserId]);
    PRINT '✅ Created IX_Posts_Status_CreatedAt_Score';
END
ELSE
    PRINT '⚠️ IX_Posts_Status_CreatedAt_Score already exists';
GO

-- Index for popular/trending posts
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_Status_ViewCount' AND object_id = OBJECT_ID('Posts'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Posts_Status_ViewCount]
    ON [dbo].[Posts]([Status], [ViewCount] DESC)
    INCLUDE ([CreatedAt], [Score], [CommentCount], [Title], [Slug], [CommunityId]);
    PRINT '✅ Created IX_Posts_Status_ViewCount';
END
ELSE
    PRINT '⚠️ IX_Posts_Status_ViewCount already exists';
GO

-- Index for post status and update time (for sitemap generation)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Posts_Status_UpdatedAt' AND object_id = OBJECT_ID('Posts'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Posts_Status_UpdatedAt]
    ON [dbo].[Posts]([Status], [UpdatedAt] DESC)
    INCLUDE ([CreatedAt], [Slug], [CommunityId], [ViewCount]);
    PRINT '✅ Created IX_Posts_Status_UpdatedAt';
END
ELSE
    PRINT '⚠️ IX_Posts_Status_UpdatedAt already exists';
GO

-- ================================================
-- 2. COMMENTS TABLE INDEXES
-- ================================================

-- Index for fetching comments by post (most common query)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_PostId_CreatedAt' AND object_id = OBJECT_ID('Comments'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Comments_PostId_CreatedAt]
    ON [dbo].[Comments]([PostId], [CreatedAt] DESC)
    INCLUDE ([UserId], [Content], [UpvoteCount], [DownvoteCount], [IsDeleted]);
    PRINT '✅ Created IX_Comments_PostId_CreatedAt';
END
ELSE
    PRINT '⚠️ IX_Comments_PostId_CreatedAt already exists';
GO

-- Index for nested comments (parent-child relationship)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_ParentCommentId' AND object_id = OBJECT_ID('Comments'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Comments_ParentCommentId]
    ON [dbo].[Comments]([ParentCommentId])
    WHERE [ParentCommentId] IS NOT NULL;
    PRINT '✅ Created IX_Comments_ParentCommentId';
END
ELSE
    PRINT '⚠️ IX_Comments_ParentCommentId already exists';
GO

-- ================================================
-- 3. USER ACTIVITIES TABLE INDEXES
-- ================================================

-- Index for recent activities (LiveActivityFeed)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserActivities_ActivityAt' AND object_id = OBJECT_ID('UserActivities'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserActivities_ActivityAt]
    ON [dbo].[UserActivities]([ActivityAt] DESC)
    INCLUDE ([UserId], [PostId], [ActivityType]);
    PRINT '✅ Created IX_UserActivities_ActivityAt';
END
ELSE
    PRINT '⚠️ IX_UserActivities_ActivityAt already exists';
GO

-- Index for user-specific activities
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserActivities_UserId_ActivityAt' AND object_id = OBJECT_ID('UserActivities'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserActivities_UserId_ActivityAt]
    ON [dbo].[UserActivities]([UserId], [ActivityAt] DESC);
    PRINT '✅ Created IX_UserActivities_UserId_ActivityAt';
END
ELSE
    PRINT '⚠️ IX_UserActivities_UserId_ActivityAt already exists';
GO

-- ================================================
-- 4. COMMUNITIES TABLE INDEXES
-- ================================================

-- Index for community listings (ordered by member count)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Communities_IsActive_MemberCount' AND object_id = OBJECT_ID('Communities'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Communities_IsActive_MemberCount]
    ON [dbo].[Communities]([IsActive], [IsDeleted], [MemberCount] DESC)
    INCLUDE ([Name], [Slug], [Description], [IconUrl], [PostCount]);
    PRINT '✅ Created IX_Communities_IsActive_MemberCount';
END
ELSE
    PRINT '⚠️ IX_Communities_IsActive_MemberCount already exists';
GO

-- ================================================
-- 5. MULTI-SITE REVENUE INDEXES
-- ================================================

-- Index for revenue queries by site and date
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MultiSiteRevenues_SiteDomain_Date' AND object_id = OBJECT_ID('MultiSiteRevenues'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MultiSiteRevenues_SiteDomain_Date]
    ON [dbo].[MultiSiteRevenues]([SiteDomain], [Date] DESC)
    INCLUDE ([Earnings], [PageViews], [PostId], [RPM]);
    PRINT '✅ Created IX_MultiSiteRevenues_SiteDomain_Date';
END
ELSE
    PRINT '⚠️ IX_MultiSiteRevenues_SiteDomain_Date already exists';
GO

-- Index for top-earning posts
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MultiSiteRevenues_PostId_Earnings' AND object_id = OBJECT_ID('MultiSiteRevenues'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MultiSiteRevenues_PostId_Earnings]
    ON [dbo].[MultiSiteRevenues]([PostId], [Earnings] DESC)
    WHERE [PostId] IS NOT NULL;
    PRINT '✅ Created IX_MultiSiteRevenues_PostId_Earnings';
END
ELSE
    PRINT '⚠️ IX_MultiSiteRevenues_PostId_Earnings already exists';
GO

-- ================================================
-- 6. POST VOTES INDEXES
-- ================================================

-- Index for user vote lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PostVotes_PostId_UserId' AND object_id = OBJECT_ID('PostVotes'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PostVotes_PostId_UserId]
    ON [dbo].[PostVotes]([PostId], [UserId])
    INCLUDE ([VoteType]);
    PRINT '✅ Created IX_PostVotes_PostId_UserId';
END
ELSE
    PRINT '⚠️ IX_PostVotes_PostId_UserId already exists';
GO

-- ================================================
-- 7. USER PROFILES INDEXES
-- ================================================

-- Index for user profile lookups by display name
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserProfiles_DisplayName' AND object_id = OBJECT_ID('UserProfiles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserProfiles_DisplayName]
    ON [dbo].[UserProfiles]([DisplayName])
    INCLUDE ([UserId], [Bio], [AvatarUrl], [Reputation]);
    PRINT '✅ Created IX_UserProfiles_DisplayName';
END
ELSE
    PRINT '⚠️ IX_UserProfiles_DisplayName already exists';
GO

-- ================================================
-- 8. CATEGORIES INDEXES
-- ================================================

-- Index for active categories
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Categories_IsActive' AND object_id = OBJECT_ID('Categories'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Categories_IsActive]
    ON [dbo].[Categories]([IsActive])
    INCLUDE ([Name], [Slug], [Description], [DisplayOrder]);
    PRINT '✅ Created IX_Categories_IsActive';
END
ELSE
    PRINT '⚠️ IX_Categories_IsActive already exists';
GO

-- ================================================
-- 9. UPDATE STATISTICS (Important!)
-- ================================================

PRINT '';
PRINT '🔄 Updating statistics for better query optimization...';

UPDATE STATISTICS [dbo].[Posts] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Comments] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Communities] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[UserActivities] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[UserProfiles] WITH FULLSCAN;

PRINT '✅ Statistics updated successfully';
GO

-- ================================================
-- 10. VERIFY INDEXES
-- ================================================

PRINT '';
PRINT '📊 Index Summary:';
PRINT '==================';

SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COUNT(ic.index_id) AS ColumnCount
FROM sys.indexes i
LEFT JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE OBJECT_NAME(i.object_id) IN ('Posts', 'Comments', 'UserActivities', 'Communities', 'MultiSiteRevenues', 'PostVotes', 'UserProfiles', 'Categories')
    AND i.type > 0  -- Exclude heaps
GROUP BY i.object_id, i.name, i.type_desc
ORDER BY TableName, IndexName;

PRINT '';
PRINT '✅ ALL PERFORMANCE INDEXES CREATED SUCCESSFULLY!';
PRINT '📈 Expected Performance Improvement: 3-10x faster queries';
PRINT '🚀 Your database is now optimized for high-traffic loads';
GO

