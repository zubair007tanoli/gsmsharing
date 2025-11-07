-- Migration: Add SEO Scoring Tables
-- Run this SQL script directly in SQL Server Management Studio or via command line

-- Create SeoScores table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SeoScores]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SeoScores] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PostId] int NOT NULL,
        [Score] decimal(5,2) NOT NULL,
        [Tier] nvarchar(20) NOT NULL DEFAULT 'Critical',
        [GoogleCompetitivenessScore] decimal(5,2) NOT NULL,
        [ContentQualityScore] decimal(5,2) NOT NULL,
        [MetaCompletenessScore] decimal(5,2) NOT NULL,
        [FreshnessScore] decimal(5,2) NOT NULL,
        [Issues] nvarchar(max) NULL,
        [RecommendedKeywords] nvarchar(max) NULL,
        [TopCompetitors] nvarchar(max) NULL,
        [PriorityRank] int NOT NULL,
        [ScoredAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [Source] nvarchar(50) NOT NULL DEFAULT 'Hybrid',
        CONSTRAINT [PK_SeoScores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SeoScores_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts] ([PostId]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_SeoScores_PostId] ON [dbo].[SeoScores] ([PostId]);
    CREATE INDEX [IX_SeoScores_Tier] ON [dbo].[SeoScores] ([Tier]);
    
    PRINT 'SeoScores table created successfully.';
END
ELSE
BEGIN
    PRINT 'SeoScores table already exists.';
END
GO

-- Create SeoOptimizationProposals table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SeoOptimizationProposals]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SeoOptimizationProposals] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PostId] int NOT NULL,
        [ProposedTitle] nvarchar(500) NULL,
        [ProposedContent] nvarchar(max) NULL,
        [ProposedMetaDescription] nvarchar(500) NULL,
        [ProposedKeywords] nvarchar(1000) NULL,
        [ChangesSummary] nvarchar(max) NULL,
        [ExpectedScoreDelta] decimal(5,2) NOT NULL,
        [CurrentScore] decimal(5,2) NOT NULL,
        [ExpectedScore] decimal(5,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT 'Pending',
        [CreatedBy] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [ReviewedBy] nvarchar(450) NULL,
        [ReviewedAt] datetime2 NULL,
        [AppliedAt] datetime2 NULL,
        [RejectionReason] nvarchar(500) NULL,
        [Source] nvarchar(50) NOT NULL DEFAULT 'Hybrid',
        CONSTRAINT [PK_SeoOptimizationProposals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SeoOptimizationProposals_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts] ([PostId]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_SeoOptimizationProposals_PostId] ON [dbo].[SeoOptimizationProposals] ([PostId]);
    CREATE INDEX [IX_SeoOptimizationProposals_Status] ON [dbo].[SeoOptimizationProposals] ([Status]);
    
    PRINT 'SeoOptimizationProposals table created successfully.';
END
ELSE
BEGIN
    PRINT 'SeoOptimizationProposals table already exists.';
END
GO

PRINT 'SEO Scoring tables migration completed!';
GO

