-- Create remaining tables that failed

-- UserActivities (Fixed cascade constraints)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserActivities')
BEGIN
    CREATE TABLE [dbo].[UserActivities] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [UserId] NVARCHAR(450) NULL,
        [SessionId] NVARCHAR(100) NULL,
        [PostId] INT NULL,
        [CommunityId] INT NULL,
        [ActivityType] NVARCHAR(50) NOT NULL,
        [TimeSpentSeconds] INT NOT NULL DEFAULT 0,
        [ScrollDepthPercent] INT NOT NULL DEFAULT 0,
        [Referrer] NVARCHAR(500) NULL,
        [DeviceType] NVARCHAR(200) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [ActivityAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [Metadata] NVARCHAR(MAX) NULL
    );
    
    CREATE INDEX [IX_UserActivities_PostId_ActivityAt] ON [UserActivities]([PostId], [ActivityAt]);
    CREATE INDEX [IX_UserActivities_UserId_ActivityAt] ON [UserActivities]([UserId], [ActivityAt]);
    CREATE INDEX [IX_UserActivities_SessionId] ON [UserActivities]([SessionId]);
    
    PRINT 'UserActivities table created';
END
ELSE
    PRINT 'UserActivities table already exists';

-- ContentRecommendations (Fixed)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContentRecommendations')
BEGIN
    CREATE TABLE [dbo].[ContentRecommendations] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [RecommendationType] NVARCHAR(100) NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [RelatedPostId] INT NULL,
        [CommunityId] INT NULL,
        [EstimatedRevenueImpact] DECIMAL(10,2) NOT NULL DEFAULT 0,
        [EstimatedTrafficImpact] DECIMAL(10,2) NOT NULL DEFAULT 0,
        [ConfidenceScore] DECIMAL(5,2) NOT NULL DEFAULT 0,
        [Priority] INT NOT NULL,
        [AnalysisData] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [ImplementedAt] DATETIME2 NULL,
        [ImplementedBy] NVARCHAR(450) NULL
    );
    
    CREATE INDEX [IX_ContentRecommendations_Status_Priority] ON [ContentRecommendations]([Status], [Priority]);
    CREATE INDEX [IX_ContentRecommendations_RelatedPostId] ON [ContentRecommendations]([RelatedPostId]);
    
    PRINT 'ContentRecommendations table created';
END
ELSE
    PRINT 'ContentRecommendations table already exists';

PRINT '✅ All tables created successfully!';

